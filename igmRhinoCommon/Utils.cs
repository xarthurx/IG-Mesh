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

        public static bool GetMesh(string fileName, out Mesh rMesh)
        {
            rMesh = new Mesh();
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryNonConstPointer(rMesh);
            var res = CppIGM.IGM_read_triangle_mesh(fileName, pMesh);

            return res;
        }

        public static bool SaveMesh(ref Mesh rMesh, string fileName)
        {
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryNonConstPointer(rMesh);
            return CppIGM.IGM_write_triangle_mesh(fileName, pMesh);

        }

        public static (List<Point3d>, List<List<int>>, Point3d, double) GetMeshInfo(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // mesh V, F
            var V = new List<Point3d>(rhinoMesh.Vertices.ToPoint3dArray());
            //var mF = rhinoMesh.Faces.ToIntArray(true);
            List<List<int>> F = new List<List<int>>();
            foreach (var f in rhinoMesh.Faces)
            {
                // check if quad mesh or not
                if (f.C == f.D)
                    F.Add(new List<int> { f.A, f.B, f.C });
                else
                    F.Add(new List<int> { f.A, f.B, f.C, f.D });

            }
            //for (int i = 0; i < mF.Length / 3; i++)
            //{
            //    F.Add(new List<int> { mF[i * 3], mF[i * 3 + 1], mF[i * 3 + 2] });
            //}

            var Ccpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            // igl for mesh centroid
            CppIGM.IGM_centroid(pMesh, Ccpp.NonConstPointer());
            var arrayCen = Ccpp.ToArray();
            Point3d cen = new Point3d(arrayCen[0], arrayCen[1], arrayCen[2]);

            // use native methods for volume
            double vol = rhinoMesh.Volume();

            return (V, F, cen, vol);
        }

        public static List<double> RemapFtoV(ref Mesh rMesh, List<double> scalarV)
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

        public static List<double> RemapVtoF(ref Mesh rMesh, List<double> scalarF)
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

        public static List<List<int>> GetAdjacencyVV(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var cppAdjVV = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            var cppAdjNum = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();

            // call the c++ func
            CppIGM.IGM_vertex_vertex_adjacency(pMesh, cppAdjVV.NonConstPointer(), cppAdjNum.NonConstPointer());

            List<List<int>> adjVV = new List<List<int>>();

            // convert to c# list
            int idCnt = 0;
            var vv = new List<int>(cppAdjVV.ToArray());
            var adjNum = new List<int>(cppAdjNum.ToArray());
            foreach (var num in adjNum)
            {
                adjVV.Add(new List<int>(vv.GetRange(idCnt, num)));
                idCnt += num;
            }

            // compute from cpp side.
            return adjVV;
        }

        public static (List<List<int>>, List<List<int>>) GetAdjacencyVT(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var cppAdjVT = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            var cppAdjVTI = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            var cppAdjNum = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();

            // call the c++ func
            CppIGM.IGM_vertex_triangle_adjacency(pMesh, cppAdjVT.NonConstPointer(), cppAdjVTI.NonConstPointer(), cppAdjNum.NonConstPointer());

            List<List<int>> adjVT = new List<List<int>>();
            List<List<int>> adjVTI = new List<List<int>>();

            // convert to c# list
            int idCnt = 0;
            var vt = new List<int>(cppAdjVT.ToArray());
            var vti = new List<int>(cppAdjVTI.ToArray());
            var adjNum = new List<int>(cppAdjNum.ToArray());
            foreach (var num in adjNum)
            {
                adjVT.Add(new List<int>(vt.GetRange(idCnt, num)));
                adjVTI.Add(new List<int>(vti.GetRange(idCnt, num)));
                idCnt += num;
            }

            // compute from cpp side.
            return (adjVT, adjVTI);
        }

        public static (List<List<int>>, List<List<int>>) GetAdjacencyTT(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var cppAdjTT = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            var cppAdjTTI = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();

            // call the c++ func
            CppIGM.IGM_triangle_triangle_adjacency(pMesh, cppAdjTT.NonConstPointer(), cppAdjTTI.NonConstPointer());

            List<List<int>> adjTT = new List<List<int>>();
            List<List<int>> adjTTI = new List<List<int>>();

            // convert to c# list
            var tt = new List<int>(cppAdjTT.ToArray());
            var tti = new List<int>(cppAdjTTI.ToArray());
            for (int i = 0; i < rMesh.Faces.Count; i++)
            {
                adjTT.Add(new List<int>(tt.GetRange(3 * i, 3)));
                adjTTI.Add(new List<int>(tti.GetRange(3 * i, 3)));
            }

            // compute from cpp side.
            return (adjTT, adjTTI);
        }


        public static List<List<int>> GetBoundaryLoop(ref Mesh rhinoMesh)
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
            CppIGM.IGM_boundary_loop(meshF, nF, boundLoopFromCpp, out int sz);

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

        public static (List<Line>, List<List<int>>, List<int>) GetBoundaryEdge(ref Mesh rhinoMesh)
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
            CppIGM.IGM_boundary_facet(meshF, nF, bndEdge, bndTriIdx, out int sz);

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

        public static List<Point3d> GetBarycenter(ref Mesh rhinoMesh)
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

        public static List<Vector3d> GetNormalsVert(ref Mesh rhinoMesh)
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

        public static List<Vector3d> GetNormalsFace(ref Mesh rhinoMesh)
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

        public static List<List<Vector3d>> GetNormalsCorner(ref Mesh rhinoMesh, double thre_deg)
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

        public static (List<Vector3d>, List<List<int>>, List<int>) GetNormalsEdge(ref Mesh rhinoMesh, int wT)
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

            return (EN, EI, EMAP);
        }

        public static List<double> GetConstrainedScalar(ref Mesh rMesh, ref List<int> con_idx, ref List<double> con_val)
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

        public static List<List<Point3d>> GetIsolineFromScalar(ref Mesh rMesh, ref List<double> meshScalar, ref List<double> iso_t)
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

        public static (List<List<Point3d>>, List<double>) GetIsolinePts(ref Mesh rMesh, ref List<int> con_idx, ref List<double> con_val, ref List<double> iso_t)
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


        public static List<float> GetLapacianScalar(ref Mesh rhinoMesh, ref List<int> con_idx, ref List<float> con_val)
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

        public static (List<Point3d> P, List<int> FI) GetRandomPointsOnMesh(ref Mesh rhinoMesh, int N, int M)
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

        public static (List<Vector3d> PD1, List<Vector3d> PD2, List<double> PV1, List<double> PV2) GetPrincipalCurvature(ref Mesh rMesh, uint r)
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

        public static List<double> GetGaussianCurvature(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            var Kcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_gaussian_curvature(pMesh, Kcpp.NonConstPointer());

            List<double> K = new List<double>(Kcpp.ToArray());
            return K;
        }

        public static List<double> GetFastWindingNumber(ref Mesh rMesh, ref List<Point3d> Q)
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

        public static (List<double> SD, List<int> FI, List<Point3d> CP) GetSignedDistance(ref Mesh rMesh, ref List<Point3d> Q, int signed_type)
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

        public static IntPtr GetHeatGeodesicPrecomputedData(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            return CppIGM.IGM_heat_geodesic_precompute(pMesh);
        }

        public static List<double> GetHeatGeodesicDist(IntPtr geoData, ref List<int> gamma)
        {
            var gammaCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt(gamma);
            var disCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();
            CppIGM.IGM_heat_geodesic_solve(geoData, gammaCpp.ConstPointer(), disCpp.NonConstPointer());

            List<double> D = new List<double>(disCpp.ToArray());

            return D;
        }

        public static List<double> GetQuadPlanarity(ref Mesh rMesh)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);
            var PlanarityCpp = new Rhino.Runtime.InteropWrappers.SimpleArrayDouble();

            CppIGM.IGM_quad_planarity(pMesh, PlanarityCpp.NonConstPointer());

            List<double> P = new List<double>(PlanarityCpp.ToArray());

            return P;
        }

        public static void PlanarizeQuadMesh(ref Mesh rMesh, int maxIter, double thres)
        {
            if (rMesh == null) throw new ArgumentNullException(nameof(rMesh));
            // notice: we will modify the mesh, so use nonConst ptr.
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rMesh);

            CppIGM.IGM_planarize_quad_mesh(pMesh, maxIter, thres);
        }

    }
}
