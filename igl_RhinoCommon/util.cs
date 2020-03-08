using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino.Geometry;

namespace IGLRhinoCommon
{
    public class Util
    {
        /// <summary>
        /// Sums two numbers
        /// </summary>
        public static double Sum(double a, double b)
        {
            return CppIGL.Add(a, b);
        }

        public static void ConvertMesh(Mesh rhino_mesh, IntPtr[] V, IntPtr[] F)
        {
            int nV = rhino_mesh.Vertices.Count;
            int nF = rhino_mesh.Faces.Count;

            for (int i = 0; i < nV; i++)
            {
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)));

            }

            //Marshal.StructureToPtr(rhino_mesh.Vertices, V, true);
            //Marshal.StructureToPtr(rhino_mesh.Faces, F, true);
        }


        public static List<List<int>> getAdjacencyLst(ref Rhino.Geometry.Mesh rhinoMesh)
        {
            // initialize the pointer and pass data
            int nF = rhinoMesh.Faces.Count;

            // copy data into the IntPtr
            //float[] V = rhino_mesh.Vertices.ToFloatArray();
            int[] F = rhinoMesh.Faces.ToIntArray(true);
            IntPtr meshF = Marshal.AllocHGlobal(Marshal.SizeOf(F[0]) * F.Length);
            Marshal.Copy(F, 0, meshF, F.Length);

            // assume each vert has most 10 neighbours
            int numEle = 3 * rhinoMesh.Vertices.Count * 20;
            IntPtr adjLstFromCpp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * numEle);

            int sz;
            _ = CppIGL.igl_adjacency_list(meshF, nF, adjLstFromCpp, out sz);

            int[] processedAdjLst = new int[numEle];
            Marshal.Copy(adjLstFromCpp, processedAdjLst, 0, numEle);
            List<List<int>> adjLst = new List<List<int>>();

            // free memory
            Marshal.FreeHGlobal(meshF);
            Marshal.FreeHGlobal(adjLstFromCpp);

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
    }
}
