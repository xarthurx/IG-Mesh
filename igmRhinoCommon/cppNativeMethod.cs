﻿using System;
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
        internal static extern void IGM_vertex_vertex_adjacency(IntPtr pMesh, IntPtr cppAdjVV, IntPtr cppAdjNum);

        /// <summary>
        /// Compute mesh vertex-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_vertex_triangle_adjacency(IntPtr pMesh, IntPtr cppAdjVT, IntPtr cppAdjVTI, IntPtr cppAdjNum);

        /// <summary>
        /// Compute mesh triangle-triangle adjacency list
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_triangle_triangle_adjacency(IntPtr pMesh, IntPtr cppAdjTT, IntPtr cppAdjTTI);

        /// <summary>
        /// Compute mesh boundary loop
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_boundary_loop(IntPtr pMesh, IntPtr bndLp, IntPtr bndNum);


        /// <summary>
        /// Compute mesh boundary edges. TODO: facets for tets
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_boundary_facet(IntPtr pMesh, IntPtr bndEdge, IntPtr bndTriIdx);

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
        internal static extern void IGM_edge_normals(IntPtr pMesh, int wT, IntPtr EN, IntPtr EI, IntPtr EMAP);
        //internal static extern void IGM_edge_normals(IntPtr V, int nV, IntPtr F, int nF, int wT, IntPtr EN, IntPtr EI, IntPtr EMAP, out int sz);

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

        /// <summary>
        /// harmonic parametrization 
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_param_harmonic(IntPtr pMesh, IntPtr Vuv, int k);


        /// <summary>
        /// compute geodesic pre-computed data
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr IGM_heat_geodesic_precompute(IntPtr pMesh);

        /// <summary>
        /// solve geodesic distance using pre-computed data
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_heat_geodesic_solve(IntPtr data, IntPtr gamma, IntPtr D);

        /// <summary>
        /// compute planarity of a quad mesh 
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_quad_planarity(IntPtr pMesh, IntPtr P);

        /// <summary>
        /// planarize a quad mesh 
        /// </summary>
        [DllImport(Import.cppLib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void IGM_planarize_quad_mesh(IntPtr pMesh, int maxIter, double thres, IntPtr oMesh);
    }

}
