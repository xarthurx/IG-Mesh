using System;
using System.Runtime.InteropServices;

namespace IGLRhinoCommon
{
    internal static class Import
    {
        //public const string cppLib = @"C:\Users\xarthur\source\repos\igl-grasshopper\x64\Release\igl_cppPort.dll";
        // use relative path
        public const string cppLib = @"igl_cppPort.dll";
    }

    internal static class CppIGL
    {
        /// <summary>
        /// Compute mesh adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_adjacency_list(IntPtr F, int nF, IntPtr adjLstFromCpp, out int sz);

        /// <summary>
        /// Compute mesh vertex-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_vertex_triangle_adjacency(int nV, IntPtr F, int nF, IntPtr adjVTFromCpp, IntPtr adjVTIFromCpp, out int sz);

        /// <summary>
        /// Compute mesh triangle-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_triangle_triangle_adjacency(IntPtr F, int nF, IntPtr adjTTFromCpp, IntPtr adjTTIFromCpp);

        /// <summary>
        /// Compute mesh boundary loop
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_boundary_loop(IntPtr F, int nF, IntPtr bndLoopFromCpp, out int sz);


        /// <summary>
        /// Compute mesh boundary edges, facets
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_boundary_facet(IntPtr F, int nF, IntPtr bndEdge, IntPtr bndTriIdx, out int sz);

        /// <summary>
        /// Compute mesh barycenters 
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_barycenter(IntPtr V, int nV, IntPtr F, int nF, IntPtr BC);

        /// <summary>
        /// Compute per_vertex and per_face normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_vert_and_face_normals(IntPtr V, int nV, IntPtr F, int nF, IntPtr VN, IntPtr FN);

        /// <summary>
        /// Compute per_corner normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_corner_normals(IntPtr V, int nV, IntPtr F, int nF, float thre_deg, IntPtr FN);

        /// <summary>
        /// Compute per_edge normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_edge_normals(IntPtr V, int nV, IntPtr F, int nF, int wT, IntPtr EN, IntPtr EI, IntPtr EMAP, out int sz);

        /// <summary>
        /// Extract mesh IsoLine Points
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void extractIsoLinePts(IntPtr V, int nV, IntPtr F, int nF,
            IntPtr con_idx, IntPtr con_val, int numCon, int divN, IntPtr isoLnPts, IntPtr numPtEachLst);

        /// <summary>
        /// Compute mesh Laplacian
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void computeLaplacian(IntPtr V, int nV, IntPtr F, int nF,
            IntPtr con_idx, IntPtr con_val, int numCon, IntPtr laplacianValue);

        /// <summary>
        /// randomly sample points on meshes.
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void igl_random_point_on_mesh(IntPtr V, int nV, IntPtr F, int nF, int N, IntPtr B, IntPtr FI);
    }
}
