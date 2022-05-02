using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino.Geometry;

namespace IGMRhinoCommon
{
    // helper func for getting double precision point array
    public static class MeshExtensions
    {
        public static double[] ToDoubleArray(this Mesh mesh)
        {
            if (!mesh.Vertices.UseDoublePrecisionVertices)
                return new double[0];

            var rc = new double[mesh.Vertices.Count * 3];
            int index = 0;
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                var pt = mesh.Vertices.Point3dAt(i);
                rc[index++] = pt.X;
                rc[index++] = pt.Y;
                rc[index++] = pt.Z;
            }
            return rc;
        }
    }
    public static class Utils
    {

        public static bool getMesh(string fileName, out Mesh rMesh)
        {
            rMesh = new Mesh();
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryNonConstPointer(rMesh);
            var res = CppIGM.IGM_read_triangle_mesh(fileName, pMesh);

            return res;
        }

        public static bool saveMesh(ref Mesh rMesh, string fileName)
        {
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryNonConstPointer(rMesh);
            return CppIGM.IGM_write_triangle_mesh(fileName, pMesh);

        }

        public static (List<Point3d>, List<List<int>>, Point3d, double) getMeshInfo(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // mesh V, F
            var V = new List<Point3d>(rhinoMesh.Vertices.ToPoint3dArray());
            var mF = rhinoMesh.Faces.ToIntArray(true);
            List<List<int>> F = new List<List<int>>();
            for (int i = 0; i < mF.Length / 3; i++)
            {
                F.Add(new List<int> { mF[i * 3], mF[i * 3 + 1], mF[i * 3 + 2] });
            }

            var Ccpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            // igl for mesh centroid
            CppIGM.IGM_centroid(pMesh, Ccpp.NonConstPointer());
            var arrayCen = Ccpp.ToArray();
            Point3d cen = new Point3d(arrayCen[0], arrayCen[1], arrayCen[2]);

            // use native methods for volume
            double vol = rhinoMesh.Volume();

            return (V, F, cen, vol);
        }

        public static List<double> remapFtoV(ref Mesh rMesh, List<double> scalarV)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var valCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(scalarV);
            var resCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            // call the cpp func
            CppIGM.IGM_remapFtoV(pMesh, valCpp.ConstPointer(), resCpp.NonConstPointer());

            // conversion to C# type
            var arrayDbl = resCpp.ToArray();
            List<double> mappedV = new List<double>(arrayDbl);

            return mappedV;
        }

        public static List<double> remapVtoF(ref Mesh rMesh, List<double> scalarF)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var valCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(scalarF);
            var resCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            // call the cpp func
            CppIGM.IGM_remapVtoF(pMesh, valCpp.ConstPointer(), resCpp.NonConstPointer());

            // conversion to C# type
            var arrayDbl = resCpp.ToArray();
            List<double> mappedF = new List<double>(arrayDbl);

            return mappedF;
        }

        public static List<List<int>> getAdjacencyLst(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            // initialize the pointer and pass data
            int nF = rhinoMesh.Faces.Count;
            int numEle = 3 * rhinoMesh.Vertices.Count * 20;

            // copy data into the IntPtr
            //float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            IntPtr adjLstFromCpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * numEle);

            // call the c++ func
            int sz;
            CppIGM.IGM_adjacency_list(meshF, nF, adjLstFromCpp, out sz);

            int[] processedAdjLst = new int[numEle];
            Marshal.Copy(adjLstFromCpp, processedAdjLst, 0, numEle);

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(adjLstFromCpp);

            List<List<int>> adjLst = new List<List<int>>();
            int cnt = 0;
            while (cnt < sz)
            {
                int num = processedAdjLst[cnt];
                cnt++;

                List<int> transferLst = new List<int>();
                for (int i = 0; i < num; i++)
                {
                    transferLst.Add(processedAdjLst[cnt++]);
                }
                adjLst.Add(transferLst);
            }

            // compute from cpp side.
            return adjLst;
        }

        public static (List<List<int>>, List<List<int>>) getAdjacencyTT(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            //IntPtr p_mesh = Rhino.Runtime.Interop.NativeGeometryNonConstPointer(rhinoMesh);

            //// initialize the pointer and pass data
            //int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;

            //// copy data into the IntPtr
            ////float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            IntPtr adjTT_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 3 * nF);
            IntPtr adjTTI_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 3 * nF);

            // call the c++ func
            CppIGM.IGM_triangle_triangle_adjacency(meshF, nF, adjTT_cpp, adjTTI_cpp);

            int[] resTT = new int[nF * 3];
            int[] resTTI = new int[nF * 3];
            Marshal.Copy(adjTT_cpp, resTT, 0, nF * 3);
            Marshal.Copy(adjTTI_cpp, resTTI, 0, nF * 3);

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(adjTT_cpp);
            Marshal.FreeHGlobal(adjTTI_cpp);

            List<List<int>> adjTT = new List<List<int>>();
            List<List<int>> adjTTI = new List<List<int>>();

            for (int i = 0; i < nF; i++)
            {
                adjTT.Add(new List<int> { resTT[i * 3], resTT[i * 3 + 1], resTT[i * 3 + 2] });
                adjTTI.Add(new List<int> { resTTI[i * 3], resTTI[i * 3 + 1], resTTI[i * 3 + 2] });
            }

            // compute from cpp side.
            return (adjTT, adjTTI);
        }

        public static (List<List<int>>, List<List<int>>) getAdjacencyVT(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            // initialize the pointer and pass data
            int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;

            int nEle = nV * 20; // assume vertex valance at most 20

            // copy data into the IntPtr
            //float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            IntPtr adjVT_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * nEle);
            IntPtr adjVTI_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * nEle);

            // call the c++ func
            int sz;
            CppIGM.IGM_vertex_triangle_adjacency(nV, meshF, nF, adjVT_cpp, adjVTI_cpp, out sz);

            int[] resVT = new int[nEle];
            int[] resVTI = new int[nEle];
            Marshal.Copy(adjVT_cpp, resVT, 0, nEle);
            Marshal.Copy(adjVTI_cpp, resVTI, 0, nEle);

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(adjVT_cpp);
            Marshal.FreeHGlobal(adjVTI_cpp);

            List<List<int>> adjVT = new List<List<int>>();
            List<List<int>> adjVTI = new List<List<int>>();
            int cnt = 0;
            while (cnt < sz)
            {
                int num = resVT[cnt];
                cnt++;

                List<int> tmpVT = new List<int>();
                List<int> tmpVTI = new List<int>();
                for (int i = 0; i < num; i++)
                {
                    tmpVT.Add(resVT[cnt]);
                    tmpVTI.Add(resVTI[cnt]);
                    cnt++;
                }
                adjVT.Add(tmpVT);
                adjVTI.Add(tmpVTI);
            }

            // compute from cpp side.
            return (adjVT, adjVTI);

        }

        public static List<List<int>> getBoundaryLoop(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            // initialize the pointer and pass data
            int nF = rhinoMesh.Faces.Count;
            int numEle = 3 * rhinoMesh.Vertices.Count * 20;

            // copy data into the IntPtr
            //float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            IntPtr boundLoopFromCpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * numEle);

            // call the c++ func
            int sz;
            CppIGM.IGM_boundary_loop(meshF, nF, boundLoopFromCpp, out sz);

            int[] processedBoundLoop = new int[numEle];
            Marshal.Copy(boundLoopFromCpp, processedBoundLoop, 0, numEle);

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(boundLoopFromCpp);

            List<List<int>> boundLoop = new List<List<int>>();
            int cnt = 0;
            while (cnt < sz)
            {
                int num = processedBoundLoop[cnt];
                cnt++;

                List<int> transferLst = new List<int>();
                for (int i = 0; i < num; i++)
                {
                    transferLst.Add(processedBoundLoop[cnt++]);
                }
                boundLoop.Add(transferLst);
            }

            // compute from cpp side.
            return boundLoop;
        }

        public static (List<Line>, List<List<int>>, List<int>) getBoundaryEdge(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            // initialize the pointer and pass data
            int nF = rhinoMesh.Faces.Count;

            // copy data into the IntPtr
            //float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            IntPtr bndEdge = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 2 * nF);
            IntPtr bndTriIdx = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * nF);

            // call the c++ func
            int sz;
            CppIGM.IGM_boundary_facet(meshF, nF, bndEdge, bndTriIdx, out sz);

            int[] resBndEdge = new int[nF * 2];
            Marshal.Copy(bndEdge, resBndEdge, 0, nF * 2);
            int[] resBndTriIdx = new int[nF];
            Marshal.Copy(bndTriIdx, resBndTriIdx, 0, nF);

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(bndEdge);
            Marshal.FreeHGlobal(bndTriIdx);

            // convert back to C# types
            List<Line> edgeGeo = new List<Line>();
            List<List<int>> boundEdge = new List<List<int>>();
            List<int> boundTriIdx = new List<int>();
            for (int i = 0; i < sz; i++)
            {
                int pt0 = resBndEdge[i * 2];
                int pt1 = resBndEdge[i * 2 + 1];
                edgeGeo.Add(new Line(rhinoMesh.Vertices[pt0], rhinoMesh.Vertices[pt1]));
                boundEdge.Add(new List<int> { pt0, pt1 });
                boundTriIdx.Add(resBndTriIdx[i]);
            }
            return (edgeGeo, boundEdge, boundTriIdx);
        }

        public static List<Point3d> getBarycenter(ref Mesh rhinoMesh)
        {
            //initialize the pointer and pass data
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var BCcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            CppIGM.IGM_barycenter(pMesh, BCcpp.NonConstPointer());

            // conversion to C# type
            var arrayPt = BCcpp.ToArray();
            List<Point3d> BC = new List<Point3d>();
            foreach (var item in arrayPt)
            {
                BC.Add(item);
            }

            return BC;
        }

        public static List<Vector3d> getNormalsVert(ref Mesh rhinoMesh)
        {

            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var VNcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            CppIGM.IGM_vert_normals(pMesh, VNcpp.NonConstPointer());

            // conversion to C# type
            var arrayPt = VNcpp.ToArray();
            List<Vector3d> VN = new List<Vector3d>();
            foreach (var item in arrayPt)
            {
                VN.Add(new Vector3d(item));
            }

            return VN;
        }

        public static List<Vector3d> getNormalsFace(ref Mesh rhinoMesh)
        {

            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var FNcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            CppIGM.IGM_face_normals(pMesh, FNcpp.NonConstPointer());

            // conversion to C# type
            var arrayPt = FNcpp.ToArray();
            List<Vector3d> FN = new List<Vector3d>();
            foreach (var item in arrayPt)
            {
                FN.Add(new Vector3d(item));
            }

            return FN;
        }

        public static List<List<Vector3d>> getNormalsCorner(ref Mesh rhinoMesh, double thre_deg)
        {

            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var CNcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            CppIGM.IGM_corner_normals(pMesh, thre_deg, CNcpp.NonConstPointer());

            List<List<Vector3d>> CN = new List<List<Vector3d>>();
            var arrayN = CNcpp.ToArray();
            for (int i = 0; i < arrayN.Length / 3; i++)
            {
                CN.Add(new List<Vector3d> { new Vector3d(arrayN[i * 3]),
                                            new Vector3d(arrayN[i * 3 + 1]),
                                            new Vector3d(arrayN[i * 3 + 2]) });
            }

            return CN;

        }

        public static (List<Vector3d>, List<List<int>>, List<int>) getNormalsEdge(ref Mesh rhinoMesh, int wT)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var ENcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            var EIcpp = new Rhino.Runtime.InteropWrappers.SimpleArray2dex();
            var EMAPcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            CppIGM.IGM_edge_normals(pMesh, wT, ENcpp.NonConstPointer(), EIcpp.NonConstPointer(), EMAPcpp.NonConstPointer());

            var arrayEN = ENcpp.ToArray();
            List<Vector3d> EN = new List<Vector3d>();
            foreach (var it in arrayEN)
            {
                EN.Add(new Vector3d(it));
            }

            var arrayEI = EIcpp.ToArray();
            var EI = new List<List<int>>();
            foreach (var it in arrayEI)
            {
                EI.Add(new List<int>() { it.I, it.J });
            }

            var EMAP = new List<int>(EMAPcpp.ToArray());



            //initialize the pointer and pass data
            //int nV = rhinoMesh.Vertices.Count;
            //int nF = rhinoMesh.Faces.Count;
            //int nE = rhinoMesh.TopologyEdges.Count * 2;

            ////// copy data into the intptr
            ////float[] v = rhinomesh.vertices.tofloatarray();
            ////intptr meshv = marshal.allochglobal(marshal.sizeof(typeof(float)) * v.length);
            ////marshal.copy(v, 0, meshv, v.length);

            ////int[] f = rhinomesh.faces.tointarray(true);
            ////intptr meshf = marshal.allochglobal(marshal.sizeof(typeof(int)) * f.length);
            ////marshal.copy(f, 0, meshf, f.length);

            ////// call the cpp func
            ////intptr en_cpp = marshal.allochglobal(marshal.sizeof(typeof(float)) * 3 * ne);
            ////intptr ei_cpp = marshal.allochglobal(marshal.sizeof(typeof(int)) * 2 * ne);
            ////intptr emap_cpp = marshal.allochglobal(marshal.sizeof(typeof(int)) * ne);

            //int sz;
            //CppIGM.IGM_edge_normals(meshV, nV, meshF, nF, wT, EN_cpp, EI_cpp, EMAP_cpp, out sz);

            //float[] resEN = new float[nE * 3];
            //int[] resEI = new int[nE * 2];
            //int[] resEMAP = new int[nE];
            //Marshal.Copy(EN_cpp, resEN, 0, nE * 3);
            //Marshal.Copy(EMAP_cpp, resEMAP, 0, nE);

            //if (meshV != IntPtr.Zero) Marshal.FreeHGlobal(meshV);
            //if (meshF != IntPtr.Zero) Marshal.FreeHGlobal(meshF);
            //if (EN_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EN_cpp);
            //if (EI_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EI_cpp);
            //if (EMAP_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EMAP_cpp);

            // send back to RhinoCommon type
            //List<Vector3d> EN = new List<Vector3d>();
            //List<List<int>> EI = new List<List<int>>();
            //List<int> EMAP = new List<int>(EMAPcpp);

            //for (int i = 0; i < sz; i++)
            //{
            //    EN.Add(new Vector3d(resEN[i * 3], resEN[i * 3 + 1], resEN[i * 3 + 2]));
            //    EI.Add(new List<int> { resEI[i * 2], resEI[i * 2 + 1] });
            //}

            return (EN, EI, EMAP);
        }

        public static List<double> getConstrainedScalar(ref Mesh rMesh, ref List<int> con_idx, ref List<double> con_val)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            if (con_idx.Count != con_val.Count) throw new OverflowException();

            // input
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);
            var conIcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt(con_idx);
            var conVcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(con_val);

            // output
            var scalarVcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_constrained_scalar(pMesh, conIcpp.ConstPointer(), conVcpp.ConstPointer(), scalarVcpp.NonConstPointer());

            // conversion to C# type
            List<double> scalarV = new List<double>(scalarVcpp.ToArray());

            return scalarV;
        }

        public static List<List<Point3d>> getIsolineFromScalar(ref Mesh rMesh, ref List<double> meshScalar, ref List<double> iso_t)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));

            // input
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);
            var meshS = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(meshScalar);
            var isoTcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(iso_t);

            // output
            var isoPcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayArrayPoint3d();

            CppIGM.IGM_extract_isoline_from_scalar(pMesh, meshS.ConstPointer(), isoTcpp.ConstPointer(), isoPcpp.NonConstPointer());

            // conversion to C# type
            List<List<Point3d>> isoP = new List<List<Point3d>>();

            for (int i = 0; i < isoPcpp.Count; i++)
            {
                isoP.Add(new List<Point3d>());

                for (int j = 0; j < isoPcpp.PointCountAt(i); j++)
                {
                    isoP[i].Add(isoPcpp[i, j]);
                }
            }

            return isoP;
        }

        public static (List<List<Point3d>>, List<double>) getIsolinePts(ref Mesh rMesh, ref List<int> con_idx, ref List<double> con_val, ref List<double> iso_t)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            if (con_idx.Count != con_val.Count) throw new OverflowException();

            // input
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);
            var conIcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt(con_idx);
            var conVcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(con_val);
            var isoTcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(iso_t);

            // output
            var isoPcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayArrayPoint3d();
            var scalarVcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_extract_isoline(pMesh, conIcpp.ConstPointer(), conVcpp.ConstPointer(), isoTcpp.ConstPointer(), isoPcpp.NonConstPointer(), scalarVcpp.NonConstPointer());

            // conversion to C# type
            List<double> scalarV = new List<double>(scalarVcpp.ToArray());
            List<List<Point3d>> isoP = new List<List<Point3d>>();

            for (int i = 0; i < isoPcpp.Count; i++)
            {
                isoP.Add(new List<Point3d>());

                for (int j = 0; j < isoPcpp.PointCountAt(i); j++)
                {
                    isoP[i].Add(isoPcpp[i, j]);
                }
            }

            return (isoP, scalarV);
        }


        public static List<float> getLapacianScalar(ref Mesh rhinoMesh, ref List<int> con_idx, ref List<float> con_val)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            float[] V = rhinoMesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;

            // data transformation and memory allocation
            IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(V[0]) * V.Length);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            IntPtr conIdx = Marshal.AllocHGlobal(Marshal.SizeOf(con_idx[0]) * con_idx.Count);
            IntPtr conVal = Marshal.AllocHGlobal(Marshal.SizeOf(con_val[0]) * con_val.Count);
            Marshal.Copy(V, 0, meshV, V.Length);
            Marshal.Copy(F, 0, meshF, F.Length);
            Marshal.Copy(con_idx.ToArray(), 0, conIdx, con_idx.Count);
            Marshal.Copy(con_val.ToArray(), 0, conVal, con_val.Count);

            // since we don't know the # of pts in a isoLine, let's assme # = V at most
            int assumedDataNum = Marshal.SizeOf(typeof(float)) * 3 * nV;
            IntPtr laplacianValue = Marshal.AllocHGlobal(assumedDataNum / 3);

            //CppIGM
            CppIGM.computeLaplacian(meshV, nV, meshF, nF, conIdx, conVal, con_idx.Count, laplacianValue);

            // process returned data
            float[] processedScalarValue = new float[nV];
            Marshal.Copy(laplacianValue, processedScalarValue, 0, nV);

            List<float> laplacianV = new List<float>();
            for (int i = 0; i < nV; i++)
            {
                laplacianV.Add(processedScalarValue[i]);
            }

            // free memory
            Marshal.FreeHGlobal(meshV);
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(conIdx);
            Marshal.FreeHGlobal(conVal);
            Marshal.FreeHGlobal(laplacianValue);

            return laplacianV;
        }

        public static (List<Point3d> P, List<int> FI) getRandomPointsOnMesh(ref Mesh rhinoMesh, int N, int M)
        {

            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var Pcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            var FIcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();

            // compute random sampling based on methods
            if (M == 0)
                CppIGM.IGM_random_point_on_mesh(pMesh, N, Pcpp.NonConstPointer(), FIcpp.NonConstPointer());
            else if (M == 1)
                CppIGM.IGM_blue_noise_sampling_on_mesh(pMesh, N, Pcpp.NonConstPointer(), FIcpp.NonConstPointer());

            List<Point3d> P = new List<Point3d>(Pcpp.ToArray());
            List<int> FI = new List<int>(FIcpp.ToArray());

            return (P, FI);
        }

        public static (List<Vector3d> PD1, List<Vector3d> PD2, List<double> PV1, List<double> PV2) getPrincipalCurvature(ref Mesh rMesh, uint r)
        {

            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            // call the cpp func
            var PD1cpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            var PD2cpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            var PV1cpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            var PV2cpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_principal_curvature(pMesh, r, PD1cpp.NonConstPointer(), PD2cpp.NonConstPointer(), PV1cpp.NonConstPointer(), PV2cpp.NonConstPointer());

            // conversion to C# type
            var arrayPD1 = PD1cpp.ToArray();
            var arrayPD2 = PD2cpp.ToArray();

            List<Vector3d> PD1 = new List<Vector3d>();
            List<Vector3d> PD2 = new List<Vector3d>();

            foreach (var item in arrayPD1)
            {
                PD1.Add(new Vector3d(item));
            }

            foreach (var item in arrayPD2)
            {
                PD2.Add(new Vector3d(item));
            }

            List<double> PV1 = new List<double>(PV1cpp.ToArray());
            List<double> PV2 = new List<double>(PV2cpp.ToArray());

            return (PD1, PD2, PV1, PV2);
        }

        public static List<double> getGaussianCurvature(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var Kcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_gaussian_curvature(pMesh, Kcpp.NonConstPointer());

            List<double> K = new List<double>(Kcpp.ToArray());
            return K;
        }

        public static List<double> getFastWindingNumber(ref Mesh rMesh, ref List<Point3d> Q)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            List<double> Qarray = new List<double>();
            foreach (var item in Q)
            {
                Qarray.Add(item.X);
                Qarray.Add(item.Y);
                Qarray.Add(item.Z);
            }
            var Qcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(Qarray);
            var Wcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_fast_winding_number(pMesh, Qcpp.ConstPointer(), Wcpp.NonConstPointer());

            List<double> W = new List<double>(Wcpp.ToArray());

            return W;
        }

        public static (List<double> SD, List<int> FI, List<Point3d> CP) getSignedDistance(ref Mesh rMesh, ref List<Point3d> Q, int signed_type)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            List<double> Qarray = new List<double>();
            foreach (var item in Q)
            {
                Qarray.Add(item.X);
                Qarray.Add(item.Y);
                Qarray.Add(item.Z);
            }
            var Qcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble(Qarray);

            var SDcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            var FIcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            var CPcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();

            CppIGM.IGM_signed_distance(pMesh, Qcpp.ConstPointer(), signed_type, SDcpp.NonConstPointer(), FIcpp.NonConstPointer(), CPcpp.NonConstPointer());

            List<double> SD = new List<double>(SDcpp.ToArray());
            List<int> FI = new List<int>(FIcpp.ToArray());
            List<Point3d> CP = new List<Point3d>(CPcpp.ToArray());

            return (SD, FI, CP);
        }

        public static void getHeatGeodesicPrecomputedData(ref Mesh rMesh, ref GeoData geoData)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);
            IntPtr geoDataCpp = Rhino.Runtime.InteropWrappers.SimpleArraySurfacePointer();

            CppIGM.IGM_heat_geodesic_precompute(pMesh, geoData);
        }

        public static List<double> getHeatGeodesicDist(IntPtr geoData, ref List<int> gamma)
        {
            var gammaCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt(gamma);
            var disCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            CppIGM.IGM_heat_geodesic_solve(geoData, gammaCpp.ConstPointer(), disCpp.NonConstPointer());

            List<double> D = new List<double>(disCpp.ToArray());

            return D;
        }
    }
}
