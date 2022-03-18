#pragma once

#include "stdafx.h"
#include <Eigen/Core>

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

void convertArrayToEigenXd(double* inputArray, int sz,
  Eigen::MatrixXd& outputEigen);
void convertArrayToEigenXf(float* inputArray, int sz,
  Eigen::MatrixXf& outputEigen);
void convertArrayToEigenXi(int* inputArray, int sz,
  Eigen::MatrixXi& outputEigen);

// Inputs & Outputs:
// V    Flattened #V x 3 matrix of vertex cordinates
// nV   vertex number
// F    Flattened #F x 3 matrix of indices of triangle corners into V
// nF   face number


// ! adjacency funcs
RH_C_FUNCTION
void igl_adjacency_list(int* F, int nF, int* adjLst, int& sz);

RH_C_FUNCTION
void igl_vertex_triangle_adjacency(int nV, int* F, int nF, int* adjVF, int* adjVFI, int& sz);

RH_C_FUNCTION
void igl_triangle_triangle_adjacency(int* F, int nF, int* adjTT, int* adjTTI);

RH_C_FUNCTION
void igl_boundary_loop(int* F, int nF, int* adjLst, int& sz);

RH_C_FUNCTION
void igl_boundary_facet(int* F, int nF, int* edge, int* triIdxLst, int& sz);


// ! property funcs
// BC   barycenters of the mesh triangles
//RH_C_FUNCTION
//void igl_barycenter(float* V, int nV, int* F, int nF, float* BC);

RH_C_FUNCTION
void igl_centroid(ON_Mesh* pMesh, ON_3dPointArray* c);


RH_C_FUNCTION
void igl_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC);
//void igl_barycenterMesh(ON_Mesh* pMesh, ON_3dPointArray* BC);

// ! normals
// VN   vertex normals
// FN   face normals
RH_C_FUNCTION
void igl_vert_and_face_normals(float* V, int nV, int* F, int nF, float* VN, float* FN);

RH_C_FUNCTION
void igl_corner_normals(float* V, int nV, int* F, int nF, float threshold_deg, float* FN);

RH_C_FUNCTION
void igl_edge_normals(float* V, int nV, int* F, int nF, int weightingType, float* EN, int* EI, int* EMAP, int& sz);


// ! advanced
RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF, int* con_idx,
  float* con_value, int numCon, int divN,
  float* isoLnPts, int* numPtsPerLst);

RH_C_FUNCTION
void computeLaplacian(float* V, int nV, int* F, int nF, int* con_idx,
  float* con_value, int numCon, float* laplacianValue);


RH_C_FUNCTION
void igl_random_point_on_mesh(float* V, int nV, int* F, int nF, int N, float* B, int* FI);
