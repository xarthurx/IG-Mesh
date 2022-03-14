using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino.Geometry;

namespace IGLRhinoCommon
{
    public static class Utils
    {
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
            CppIGL.igl_adjacency_list(meshF, nF, adjLstFromCpp, out sz);

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
            CppIGL.igl_boundary_loop(meshF, nF, boundLoopFromCpp, out sz);

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

        public static List<Point3d> getBarycenter(ref Mesh rhinoMesh)
        {
            if (rhinoMesh == null) throw new ArgumentNullException(nameof(rhinoMesh));

            //initialize the pointer and pass data
            int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;

            // copy data into the IntPtr
            float[] V = rhinoMesh.Vertices.ToFloatArray();
            IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(V[0]) * V.Length);
            Marshal.Copy(V, 0, meshV, V.Length);


            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // call the cpp func
            IntPtr BCfromCpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * nF);
            CppIGL.igl_barycenter(meshV, nV, meshF, nF, BCfromCpp);

            float[] processedBC = new float[nF * 3];
            Marshal.Copy(BCfromCpp, processedBC, 0, nF * 3);

            Marshal.FreeHGlobal(meshV);
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(BCfromCpp);

            // send back to Rhino Common type
            List<Point3d> BC = new List<Point3d>();
            for (int i = 0; i < nF; i++)
            {
                BC.Add(new Point3d(processedBC[i * 3], processedBC[i * 3 + 1], processedBC[i * 3 + 2]));
            }

            return BC;
        }

        public static (List<Vector3d> VN, List<Vector3d> FN) getNormals(ref Mesh rhinoMesh)
        {
            //initialize the pointer and pass data
            int nV = rhinoMesh.Vertices.Count;
            int nF = rhinoMesh.Faces.Count;

            // copy data into the IntPtr
            float[] V = rhinoMesh.Vertices.ToFloatArray();
            IntPtr meshV = Marshal.AllocHGlobal(Marshal.SizeOf(V[0]) * V.Length);
            Marshal.Copy(V, 0, meshV, V.Length);

            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // call the cpp func
            IntPtr VN_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * nV);
            IntPtr FN_cpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(float)) * 3 * nF);
            CppIGL.igl_normals(meshV, nV, meshF, nF, VN_cpp, FN_cpp);

            float[] processedVN = new float[nV * 3];
            float[] processedFN = new float[nF * 3];
            Marshal.Copy(VN_cpp, processedVN, 0, nV * 3);
            Marshal.Copy(FN_cpp, processedFN, 0, nF * 3);

            Marshal.FreeHGlobal(meshV);
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(VN_cpp);
            Marshal.FreeHGlobal(FN_cpp);

            // send back to RhinoCommon type
            List<Vector3d> VN = new List<Vector3d>();
            List<Vector3d> FN = new List<Vector3d>();

            for (int i = 0; i < nV; i++)
            {
                VN.Add(new Vector3d(processedVN[i * 3], processedVN[i * 3 + 1], processedVN[i * 3 + 2]));
            }

            for (int i = 0; i < nF; i++)
            {
                FN.Add(new Vector3d(processedFN[i * 3], processedFN[i * 3 + 1], processedFN[i * 3 + 2]));
            }

            return (VN, FN);

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

    }
}
