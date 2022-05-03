#pragma once
#include "stdafx.h"

#if defined(RH_DLL_EXPORTS)

/* Compiling XIGLLIB as a Windows DLL - export classes, functions, and globals
 */
#define RH_CPP_CLASS __declspec(dllexport)
#define RH_CPP_FUNCTION __declspec(dllexport)
#define RH_CPP_DATA __declspec(dllexport)

#define RH_C_FUNCTION extern "C" __declspec(dllexport)

#else

/* Using XIGLLIB as a Windows DLL - import classes, functions, and globals */
#define RH_CPP_CLASS __declspec(dllimport)
#define RH_CPP_FUNCTION __declspec(dllimport)
#define RH_CPP_DATA __declspec(dllimport)

#define RH_C_FUNCTION extern "C" __declspec(dllimport)

#endif

// ! --------------------------------
// ! IO funcs
// ! --------------------------------
// construct openNURBS mesh directly in cpp and send it back to C#
RH_C_FUNCTION
void IGM_read_triangle_mesh(char* filename, ON_Mesh* pMesh);

RH_C_FUNCTION
bool IGM_write_triangle_mesh(char* filename, ON_Mesh* pMesh);

// ! --------------------------------
// ! property funcs
// ! --------------------------------

RH_C_FUNCTION
void IGM_centroid(ON_Mesh* pMesh, ON_SimpleArray<double>* c);

// BC   barycenters of the mesh triangles
RH_C_FUNCTION
void IGM_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC);

// ! --------------------------------
// ! adjacency funcs
// ! --------------------------------
RH_C_FUNCTION
void IGM_adjacency_list(int* F, int nF, int* adjLst, int& sz);

RH_C_FUNCTION
void IGM_vertex_triangle_adjacency(int nV, int* F, int nF, int* adjVF,
                                   int* adjVFI, int& sz);

RH_C_FUNCTION
void IGM_triangle_triangle_adjacency(int* F, int nF, int* adjTT, int* adjTTI);

RH_C_FUNCTION
void IGM_boundary_loop(int* F, int nF, int* adjLst, int& sz);
// void IGM_boundary_loop(ON_Mesh* pMesh, int* adjLst, int& sz);

RH_C_FUNCTION
void IGM_boundary_facet(int* F, int nF, int* edge, int* triIdxLst, int& sz);

// ! --------------------------------
// ! normal funcs
// ! --------------------------------
/*
  due to the incomplete of Rhino.Runtime.InteropWrappers,
  we use pointarray to handle vectors
*/
// ! VN   vertex normals
RH_C_FUNCTION
void IGM_vert_normals(ON_Mesh* pMesh, ON_3dPointArray* VN);

// ! FN   face normals
RH_C_FUNCTION
void IGM_face_normals(ON_Mesh* pMesh, ON_3dPointArray* FN);

RH_C_FUNCTION
void IGM_corner_normals(ON_Mesh* pMesh, const double threshold_deg,
                        ON_3dPointArray* CN);

RH_C_FUNCTION
void IGM_edge_normals(ON_Mesh* pMesh, int weightingType, ON_3dPointArray* EN,
                      ON_SimpleArray<ON_2dex>* EI, ON_SimpleArray<int>* EMAP);

// ! --------------------------------
// ! mapping
// ! --------------------------------

RH_C_FUNCTION
void IGM_remapFtoV(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res);

RH_C_FUNCTION
void IGM_remapVtoF(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res);

// ! --------------------------------
// ! measure
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
// ! utils
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

static igl::HeatGeodesicsData<double> geoData;

RH_C_FUNCTION
igl::HeatGeodesicsData<double>* IGM_heat_geodesic_precompute(ON_Mesh* pMesh);

RH_C_FUNCTION
void IGM_heat_geodesic_solve(igl::HeatGeodesicsData<double>* data,
                             ON_SimpleArray<int>* gamma,
                             ON_SimpleArray<double>* D);
