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

// void cvtONstructToEigen(const ON_3dPointArray& mV,
//                            const ON_SimpleArray<ON_MeshFace>& mF,
//                            MatrixXd& matV, MatrixXi& matF);
// void cvtEigenToON_Points(const MatrixXd& matP, ON_3dPointArray* P);
// void cvtEigenToON_Vector(const MatrixXd& matV, ON_3dVectorArray* V);

// Inputs & Outputs:
// V    Flattened #V x 3 matrix of vertex cordinates
// nV   vertex number
// F    Flattened #F x 3 matrix of indices of triangle corners into V
// nF   face number

// ! --------------------------------
// ! IO funcs
// ! --------------------------------
// RH_C_FUNCTION
// bool IGM_read_triangle_mesh(char* filename, ON_3dPointArray* V,
//                            ON_SimpleArray<int>* F);

// construct openNURBS mesh directly in cpp and send it back to C#
RH_C_FUNCTION
void IGM_read_triangle_mesh(char* filename, ON_Mesh* pMesh);

// ! --------------------------------
// ! property funcs
// ! --------------------------------

RH_C_FUNCTION
void IGM_centroid(ON_Mesh* pMesh, ON_3dPointArray* c);

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
void IGM_corner_normals(ON_Mesh* pMesh, double threshold_deg,
                        ON_3dPointArray* CN);
// void IGM_corner_normals(float* V, int nV, int* F, int nF, float
// threshold_deg,
//                        float* FN);

RH_C_FUNCTION
void IGM_edge_normals(float* V, int nV, int* F, int nF, int weightingType,
                      float* EN, int* EI, int* EMAP, int& sz);

// ! --------------------------------
// ! advanced
// ! --------------------------------
RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF, int* con_idx,
                       float* con_value, int numCon, int divN, float* isoLnPts,
                       int* numPtsPerLst);

RH_C_FUNCTION
void computeLaplacian(float* V, int nV, int* F, int nF, int* con_idx,
                      float* con_value, int numCon, float* laplacianValue);

RH_C_FUNCTION
void IGM_random_point_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* B,
                              ON_SimpleArray<int>* FI);

RH_C_FUNCTION
void IGM_principal_curvature(ON_Mesh* pMesh, unsigned r, ON_3dPointArray* PD1,
                             ON_3dPointArray* PD2, ON_SimpleArray<double>* PV1,
                             ON_SimpleArray<double>* PV2);
