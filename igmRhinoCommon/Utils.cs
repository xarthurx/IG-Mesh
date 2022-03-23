﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino.Geometry;

namespace IGLRhinoCommon
{
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

            var cen = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            // igl for mesh centroid
            CppIGL.IGM_centroid(pMesh, cen.NonConstPointer());

            // use native methods for volume
            double vol = rhinoMesh.Volume();

            return (V, F, cen.ToArray()[0], vol);
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
            CppIGL.IGM_adjacency_list(meshF, nF, adjLstFromCpp, out sz);

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
            int sz;
            CppIGL.IGM_triangle_triangle_adjacency(meshF, nF, adjTT_cpp, adjTTI_cpp);

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
            CppIGL.IGM_vertex_triangle_adjacency(nV, meshF, nF, adjVT_cpp, adjVTI_cpp, out sz);

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
            CppIGL.IGM_boundary_loop(meshF, nF, boundLoopFromCpp, out sz);

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
            CppIGL.IGM_boundary_facet(meshF, nF, bndEdge, bndTriIdx, out sz);

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

        //public static List<Point3d> getBarycenter(ref Mesh rhinoMesh)
        //{
        //    if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

        //    //initialize the pointer and pass data
        //    int nV = rhinoMesh.Vertices.Count;
        //    int nF = rhinoMesh.Faces.Count;

        //    // copy data into the IntPtr
        //    float[] V = rhinoMesh.Vertices.ToFloatArray();
        //    IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * V.Length);
        //    Marshal.Copy(V, 0, meshV, V.Length);


        //    int[] F = rhinoMesh.Faces.ToIntArray(true);
        //    IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * F.Length);
        //    Marshal.Copy(F, 0, meshF, F.Length);

        //    // call the cpp func
        //    IntPtr BCfromCpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * nF);
        //    CppIGL.IGM_barycenter(meshV, nV, meshF, nF, BCfromCpp);

        //    float[] processedBC = new float[nF * 3];
        //    Marshal.Copy(BCfromCpp, processedBC, 0, nF * 3);

        //    Marshal.FreeHGlobal(meshV);
        //    Marshal.FreeHGlobal(meshF);
        //    Marshal.FreeHGlobal(BCfromCpp);

        //    // send back to Rhino Common type
        //    List<Point3d> BC = new List<Point3d>();
        //    for (int i = 0; i < nF; i++)
        //    {
        //        BC.Add(new Point3d(processedBC[i * 3], processedBC[i * 3 + 1], processedBC[i * 3 + 2]));
        //    }

        //    return BC;
        //}

        public static List<Point3d> getBarycenter(ref Mesh rhinoMesh)
        {
            //initialize the pointer and pass data
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var BCcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            CppIGL.IGM_barycenter(pMesh, BCcpp.NonConstPointer());

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
            CppIGL.IGM_vert_normals(pMesh, VNcpp.NonConstPointer());

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
            CppIGL.IGM_face_normals(pMesh, FNcpp.NonConstPointer());

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
            CppIGL.IGM_corner_normals(pMesh, thre_deg, CNcpp.NonConstPointer());

            List<List<Vector3d>> CN = new List<List<Vector3d>>();
            var arrayN = CNcpp.ToArray();
            for (int i = 0; i < arrayN.Length / 3; i++)
            {
                CN.Add(new List<Vector3d> { new Vector3d(arrayN[i * 3]),
                                            new Vector3d(arrayN[i * 3 + 1]),
                                            new Vector3d(arrayN[i * 3 + 2]) });
            }

            return CN;

            ////initialize the pointer and pass data
            //int nV = rhinoMesh.Vertices.Count;
            //int nF = rhinoMesh.Faces.Count;

            //// copy data into the IntPtr
            //float[] V = rhinoMesh.Vertices.ToFloatArray();
            //IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(V[0]) * V.Length);
            //Marshal.Copy(V, 0, meshV, V.Length);

            //int[] F = rhinoMesh.Faces.ToIntArray(true);
            //IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            //Marshal.Copy(F, 0, meshF, F.Length);

            //// call the cpp func
            //IntPtr CN_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * 3 * nF);
            //CppIGL.IGM_corner_normals(meshV, nV, meshF, nF, thre_deg, CN_cpp);

            //float[] resCN = new float[nF * 3 * 3];
            //Marshal.Copy(CN_cpp, resCN, 0, nF * 3 * 3);

            //Marshal.FreeHGlobal(meshV);
            //Marshal.FreeHGlobal(meshF);
            //Marshal.FreeHGlobal(CN_cpp);

            //// send back to RhinoCommon type
            //List<List<Vector3d>> CN = new List<List<Vector3d>>();

            //for (int i = 0; i < nF; i++)
            //{
            //    List<Vector3d> cornerN = new List<Vector3d>();
            //    int fId = i * 3;
            //    for (int j = 0; j < 3; j++)
            //    {
            //        int nId = fId + j;
            //        cornerN.Add(new Vector3d(resCN[nId * 3], resCN[nId * 3 + 1], resCN[nId * 3 + 2]));
            //    }
            //    CN.Add(cornerN);

            //}


        }

        public static (List<Vector3d>, List<List<int>>, List<int>) getNormalsEdge(ref Mesh rhinoMesh, int wT)
        {
            //initialize the pointer and pass data
            int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;
            int nE = rhinoMesh.TopologyEdges.Count * 2;

            // copy data into the IntPtr
            float[] V = rhinoMesh.Vertices.ToFloatArray();
            IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * V.Length);
            Marshal.Copy(V, 0, meshV, V.Length);

            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // call the cpp func
            IntPtr EN_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * nE);
            IntPtr EI_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 2 * nE);
            IntPtr EMAP_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * nE);

            int sz;
            CppIGL.IGM_edge_normals(meshV, nV, meshF, nF, wT, EN_cpp, EI_cpp, EMAP_cpp, out sz);

            float[] resEN = new float[nE * 3];
            int[] resEI = new int[nE * 2];
            int[] resEMAP = new int[nE];
            Marshal.Copy(EN_cpp, resEN, 0, nE * 3);
            Marshal.Copy(EI_cpp, resEI, 0, nE * 2);
            Marshal.Copy(EMAP_cpp, resEMAP, 0, nE);

            if (meshV != IntPtr.Zero) Marshal.FreeHGlobal(meshV);
            if (meshF != IntPtr.Zero) Marshal.FreeHGlobal(meshF);
            if (EN_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EN_cpp);
            if (EI_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EI_cpp);
            if (EMAP_cpp != IntPtr.Zero) Marshal.FreeHGlobal(EMAP_cpp);

            // send back to RhinoCommon type
            List<Vector3d> EN = new List<Vector3d>();
            List<List<int>> EI = new List<List<int>>();
            List<int> EMAP = new List<int>(resEMAP);

            for (int i = 0; i < sz; i++)
            {
                EN.Add(new Vector3d(resEN[i * 3], resEN[i * 3 + 1], resEN[i * 3 + 2]));
                EI.Add(new List<int> { resEI[i * 2], resEI[i * 2 + 1] });
            }

            return (EN, EI, EMAP);
        }

        public static List<List<Point3d>> getIsolinePts(ref Mesh rhinoMesh, ref List<int> con_idx, ref List<float> con_val, int divN)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            if (con_idx.Count != con_val.Count) throw new OverflowException();

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
            IntPtr isoLinePts = Marshal.AllocHGlobal(assumedDataNum);
            IntPtr numPtsEachLst = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * divN);

            //CppIGL
            CppIGL.extractIsoLinePts(meshV, nV, meshF, nF, conIdx, conVal, con_idx.Count, divN, isoLinePts, numPtsEachLst);

            // process returned data
            int[] processedNumPtsLst = new int[divN];
            Marshal.Copy(numPtsEachLst, processedNumPtsLst, 0, divN);

            int totalNumPts = 0;
            for (int i = 0; i < divN; i++) totalNumPts += processedNumPtsLst[i];
            float[] processedIsoLine = new float[Marshal.SizeOf(typeof(float)) * 3 * nV];
            Marshal.Copy(isoLinePts, processedIsoLine, 0, totalNumPts * 3);

            // free memory
            Marshal.FreeHGlobal(meshV);
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(conIdx);
            Marshal.FreeHGlobal(conVal);
            Marshal.FreeHGlobal(numPtsEachLst);
            Marshal.FreeHGlobal(isoLinePts);

            List<List<Point3d>> isolst = new List<List<Point3d>>();

            int ptCnt = 0;
            for (int i = 0; i < divN; i++)
            {
                int sz = processedNumPtsLst[i];
                List<Point3d> ptLst = new List<Point3d>();
                for (int j = 0; j < sz; j++)
                {
                    ptLst.Add(new Point3d(processedIsoLine[ptCnt * 3],
                           processedIsoLine[ptCnt * 3 + 1],
                           processedIsoLine[ptCnt * 3 + 2]));
                    ptCnt++;
                }
                isolst.Add(ptLst);
            }

            return isolst;
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

            //CppIGL
            CppIGL.computeLaplacian(meshV, nV, meshF, nF, conIdx, conVal, con_idx.Count, laplacianValue);

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

        public static (List<Point3d> BC, List<int> FI) getRandomPointsOnMesh(ref Mesh rhinoMesh, int N)
        {

            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));
            IntPtr pMesh = Rhino.Runtime.Interop.NativeGeometryConstPointer(rhinoMesh);

            // call the cpp func
            var Pcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d();
            var FIcpp = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
            CppIGL.IGM_random_point_on_mesh(pMesh, N, Pcpp.NonConstPointer(), FIcpp.NonConstPointer());

            List<Point3d> BC = new List<Point3d>(Pcpp.ToArray());
            List<int> FI = new List<int>(FIcpp.ToArray());

            return (BC, FI);
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

            CppIGL.IGM_principal_curvature(pMesh, r, PD1cpp.NonConstPointer(), PD2cpp.NonConstPointer(), PV1cpp.NonConstPointer(), PV2cpp.NonConstPointer());

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
    }
}