#pragma once
#include "stdafx.h"

// Windows build
#if defined(_WIN32)
#define RH_CPP_CLASS __declspec(dllexport)
#define RH_CPP_FUNCTION __declspec(dllexport)
#define RH_C_FUNCTION extern "C" __declspec(dllexport)
#endif

// Apple build
#if defined(__APPLE__)
#define RH_CPP_CLASS __attribute__((visibility("default")))
#define RH_CPP_FUNCTION __attribute__((visibility("default")))
#define RH_C_FUNCTION extern "C" __attribute__((visibility("default")))
#endif  // __APPLE__

// ! testing func for cpp/c# integration
extern "C" {
__declspec(dllexport) double IGM_simple_addition(double a, double b);
}

// ! --------------------------------
// ! 01:: IO, property funcs
// ! --------------------------------
// construct openNURBS mesh directly in cpp and send it back to C#
RH_C_FUNCTION
void IGM_read_triangle_mesh(char* filename, ON_Mesh* pMesh);

RH_C_FUNCTION
bool IGM_write_triangle_mesh(char* filename, ON_Mesh* pMesh);

RH_C_FUNCTION
void IGM_centroid(ON_Mesh* pMesh, ON_SimpleArray<double>* c);

// ! --------------------------------
// ! 02:: centre, normal funcs
// ! --------------------------------
// BC   barycenters of the mesh triangles
RH_C_FUNCTION
void IGM_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC);

/*
  due to the incomplete of Rhino.Runtime.InteropWrappers,
  we use pointarray to handle vectors
*/
RH_C_FUNCTION
void IGM_vert_normals(ON_Mesh* pMesh, ON_3dPointArray* VN);

RH_C_FUNCTION
void IGM_face_normals(ON_Mesh* pMesh, ON_3dPointArray* FN);

RH_C_FUNCTION
void IGM_corner_normals(ON_Mesh* pMesh, const double threshold_deg,
                        ON_3dPointArray* CN);

RH_C_FUNCTION
void IGM_edge_normals(ON_Mesh* pMesh, int weightingType, ON_3dPointArray* EN,
                      ON_SimpleArray<ON_2dex>* EI, ON_SimpleArray<int>* EMAP);

// ! --------------------------------
// ! 03:: adjacency, bound funcs
// ! --------------------------------
RH_C_FUNCTION
void IGM_vertex_vertex_adjacency(ON_Mesh* pMesh, ON_SimpleArray<int>* adjVV,
                                 ON_SimpleArray<int>* adjNum);

RH_C_FUNCTION
void IGM_vertex_triangle_adjacency(ON_Mesh* pMesh, ON_SimpleArray<int>* adjVF,
                                   ON_SimpleArray<int>* adjVFI,
                                   ON_SimpleArray<int>* adjNum);

RH_C_FUNCTION
void IGM_triangle_triangle_adjacency(ON_Mesh* pMesh, ON_SimpleArray<int>* adjTT,
                                     ON_SimpleArray<int>* adjTTI);

RH_C_FUNCTION
void IGM_boundary_loop(ON_Mesh* pMesh, ON_SimpleArray<int>* bndLp,
                       ON_SimpleArray<int>* bndNum);

RH_C_FUNCTION
void IGM_boundary_facet(ON_Mesh* pMesh, ON_SimpleArray<int>* EL,
                        ON_SimpleArray<int>* TL);

// ! --------------------------------
// ! 04:: mapping
// ! --------------------------------

RH_C_FUNCTION
void IGM_remapFtoV(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res);

RH_C_FUNCTION
void IGM_remapVtoF(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res);

// ! --------------------------------
// ! 05:: measure
// ! --------------------------------
RH_C_FUNCTION
void IGM_laplacian(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                   ON_SimpleArray<double>* con_val,
                   ON_SimpleArray<double>* laplacianValue);

RH_C_FUNCTION
void IGM_blue_noise_sampling_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* P,
                                     ON_SimpleArray<int>* FI);

RH_C_FUNCTION
void IGM_principal_curvature(ON_Mesh* pMesh, unsigned r, ON_3dPointArray* PD1,
                             ON_3dPointArray* PD2, ON_SimpleArray<double>* PV1,
                             ON_SimpleArray<double>* PV2);

RH_C_FUNCTION
void IGM_gaussian_curvature(ON_Mesh* pMesh, ON_SimpleArray<double>* K);

RH_C_FUNCTION
void IGM_fast_winding_number(ON_Mesh* pMesh, ON_SimpleArray<double>* Q,
                             ON_SimpleArray<double>* W);

RH_C_FUNCTION
void IGM_signed_distance(ON_Mesh* pMesh, ON_SimpleArray<double>* Q, int type,
                         ON_SimpleArray<double>* S, ON_SimpleArray<int>* I,
                         ON_3dPointArray* C);

// ! --------------------------------
// ! 06:: utils
// ! --------------------------------

RH_C_FUNCTION
void IGM_constrained_scalar(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                            ON_SimpleArray<double>* con_val,
                            ON_SimpleArray<double>* meshScal);

RH_C_FUNCTION
void IGM_extract_isoline_from_scalar(ON_Mesh* pMesh,
                                     ON_SimpleArray<double>* iso_t,
                                     ON_SimpleArray<double>* meshS,
                                     ON_SimpleArray<ON_3dPointArray*>* isoP);

// combined func of the above two
RH_C_FUNCTION
void IGM_extract_isoline(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                         ON_SimpleArray<double>* con_val,
                         ON_SimpleArray<double>* iso_t,
                         ON_SimpleArray<ON_3dPointArray*>* isoP,
                         ON_SimpleArray<double>* meshS);

RH_C_FUNCTION
void IGM_random_point_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* P,
                              ON_SimpleArray<int>* FI);

// heat geodesic methods and corresponding pre-compute class
static igl::HeatGeodesicsData<double> geoData;
RH_C_FUNCTION
igl::HeatGeodesicsData<double>* IGM_heat_geodesic_precompute(ON_Mesh* pMesh);

RH_C_FUNCTION
void IGM_heat_geodesic_solve(igl::HeatGeodesicsData<double>* data,
                             ON_SimpleArray<int>* gamma,
                             ON_SimpleArray<double>* D);

// ! --------------------------------
// ! Quad Mesh Tool
// ! --------------------------------
RH_C_FUNCTION
void IGM_quad_planarity(ON_Mesh* pMesh, ON_SimpleArray<double>* P);

RH_C_FUNCTION
void IGM_planarize_quad_mesh(ON_Mesh* pMesh, int maxIter, double thres,
                             ON_Mesh* oMesh);
