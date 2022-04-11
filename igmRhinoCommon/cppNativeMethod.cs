using System;
using System.Runtime.InteropServices;

namespace IGMRhinoCommon
{
    internal static class Import
    {
        // absolute path
        //public const string cppLib = @"C:/repo/igm-GH/x64/Release/igmCppPort.dll";

        // relative path
        public const string cppLib = @"igmCppPort.dll";
    }

    internal static class CppIGM
    {
        /// <summary>
        /// Read triangle mesh
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IGM_read_triangle_mesh(string fName, IntPtr pMesh);

        /// <summary>
        /// Write triangle mesh
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool IGM_write_triangle_mesh(string fName, IntPtr pMesh);

        /// <summary>
        /// Compute mesh adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_adjacency_list(IntPtr F, int nF, IntPtr adjLstFromCpp, out int sz);

        /// <summary>
        /// Compute mesh vertex-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_vertex_triangle_adjacency(int nV, IntPtr F, int nF, IntPtr adjVTFromCpp, IntPtr adjVTIFromCpp, out int sz);

        /// <summary>
        /// Compute mesh triangle-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_triangle_triangle_adjacency(IntPtr F, int nF, IntPtr adjTTFromCpp, IntPtr adjTTIFromCpp);

        /// <summary>
        /// Compute mesh boundary loop
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_boundary_loop(IntPtr F, int nF, IntPtr bndLoopFromCpp, out int sz);


        /// <summary>
        /// Compute mesh boundary edges, facets
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_boundary_facet(IntPtr F, int nF, IntPtr bndEdge, IntPtr bndTriIdx, out int sz);

        /// <summary>
        /// Compute mesh centroid
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_centroid(IntPtr pMesh, IntPtr cen);

        /// <summary>
        /// Compute mesh barycenters 
        /// </summary>
        //[DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        //internal static extern void IGM_barycenter(IntPtr V, int nV, IntPtr F, int nF, IntPtr BC);

        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_barycenter(IntPtr pMesh, IntPtr BC);

        /// <summary>
        /// Compute per_vertex 
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_vert_normals(IntPtr pMesh, IntPtr VN);

        /// <summary>
        /// Compute  per_face normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_face_normals(IntPtr pMesh, IntPtr FN);

        /// <summary>
        /// Compute per_corner normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_corner_normals(IntPtr pMesh, double thre_deg, IntPtr CN);
        //internal static extern void IGM_corner_normals(IntPtr V, int nV, IntPtr F, int nF, float thre_deg, IntPtr FN);

        /// <summary>
        /// Compute per_edge normals
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_edge_normals(IntPtr V, int nV, IntPtr F, int nF, int wT, IntPtr EN, IntPtr EI, IntPtr EMAP, out int sz);

        /// <summary>
        /// remap scalar from face to vertex
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_remapFtoV(IntPtr pMesh, IntPtr val, IntPtr res);

        /// <summary>
        /// remap scalar from vertex to face
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_remapVtoF(IntPtr pMesh, IntPtr val, IntPtr res);

        /// <summary>
        /// Extract mesh constrained scalar field
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_constrained_scalar(IntPtr pMesh, IntPtr con_idx, IntPtr con_val, IntPtr meshScalar);

        /// <summary>
        /// Extract mesh isoline from scalar field
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_extract_isoline_from_scalar(IntPtr pMesh, IntPtr meshScalar, IntPtr iso_t, IntPtr isoP);

        /// <summary>
        /// Extract mesh IsoLine Points
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_extract_isoline(IntPtr pMesh, IntPtr con_idx, IntPtr con_val, IntPtr iso_t, IntPtr isoP, IntPtr meshScalar);

        /// <summary>
        /// Compute mesh Laplacian
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void computeLaplacian(IntPtr V, int nV, IntPtr F, int nF,
            IntPtr con_idx, IntPtr con_val, int numCon, IntPtr laplacianValue);

        /// <summary>
        /// randomly sample points on meshes
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_random_point_on_mesh(IntPtr pMesh, int N, IntPtr B, IntPtr FI);

        /// <summary>
        /// randomly sample points on meshes
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_blue_noise_sampling_on_mesh(IntPtr pMesh, int N, IntPtr P, IntPtr FI);

        /// <summary>
        /// Compute principal curvatures
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_principal_curvature(IntPtr pMesh, uint r, IntPtr PD1, IntPtr PD2, IntPtr PV1, IntPtr PV2);

        /// <summary>
        /// Compute gaussian curvatures
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_gaussian_curvature(IntPtr pMesh, IntPtr K);

        /// <summary>
        /// compute the fast winding number
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_fast_winding_number(IntPtr pMesh, IntPtr Q, IntPtr W);

        /// <summary>
        /// compute signed distance
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_signed_distance(IntPtr pMesh, IntPtr Q, int type, IntPtr S, IntPtr I, IntPtr C);
    }
}
