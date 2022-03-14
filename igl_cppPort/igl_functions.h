#pragma once

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

RH_C_FUNCTION
void igl_adjacency_list(int* F, int nF, int* adjLst, int& sz);

RH_C_FUNCTION
void igl_boundary_loop(int* F, int nF, int* adjLst, int& sz);

// BC   barycenters of the mesh triangles
RH_C_FUNCTION
void igl_barycenter(float* V, int nV, int* F, int nF, float* BC);

// VN   vertex normals
// FN   face normals
RH_C_FUNCTION
void igl_normals(float* V, int nV, int* F, int nF, float* VN, float* FN);

RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF, int* con_idx,
  float* con_value, int numCon, int divN,
  float* isoLnPts, int* numPtsPerLst);

RH_C_FUNCTION
void computeLaplacian(float* V, int nV, int* F, int nF, int* con_idx,
  float* con_value, int numCon, float* laplacianValue);
