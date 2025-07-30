#include "GeoSharPlusCPP/API/BridgeAPI.h"

#include <igl/barycenter.h>
#include <igl/centroid.h>
#include <igl/per_corner_normals.h>
#include <igl/per_edge_normals.h>
#include <igl/per_face_normals.h>
#include <igl/per_vertex_normals.h>
#include <igl/read_triangle_mesh.h>
#include <igl/write_triangle_mesh.h>

#include <iostream>
#include <memory>

#include "GSP_FB/cpp/mesh_generated.h"
#include "GSP_FB/cpp/pointArray_generated.h"
#include "GSP_FB/cpp/point_generated.h"
#include "GeoSharPlusCPP/Core/MathTypes.h"
#include "GeoSharPlusCPP/Serialization/Serializer.h"

namespace GS = GeoSharPlusCPP::Serialization;

extern "C" {

GEOSHARPLUS_API bool GEOSHARPLUS_CALL point3d_roundtrip(const uint8_t* inBuffer,
                                                        int inSize,
                                                        uint8_t** outBuffer,
                                                        int* outSize) {
  *outBuffer = nullptr;
  *outSize = 0;

  GeoSharPlusCPP::Vector3d pt;
  if (!GS::deserializePoint(inBuffer, inSize, pt)) {
    return false;
  }

  // Serialize the point into the allocated buffer
  if (!GS::serializePoint(pt, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL point3d_array_roundtrip(
    const uint8_t* inBuffer, int inSize, uint8_t** outBuffer, int* outSize) {
  *outBuffer = nullptr;
  *outSize = 0;

  std::vector<GeoSharPlusCPP::Vector3d> points;
  if (!GS::deserializePointArray(inBuffer, inSize, points)) {
    return false;
  }

  // Serialize the point array into the allocated buffer
  if (!GS::serializePointArray(points, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL mesh_roundtrip(const uint8_t* inBuffer,
                                                     int inSize,
                                                     uint8_t** outBuffer,
                                                     int* outSize) {
  *outBuffer = nullptr;
  *outSize = 0;

  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  // Serialize the mesh into the allocated buffer
  if (!GS::serializeMesh(mesh, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_read_triangle_mesh(
    const char* filename, uint8_t** outBuffer, int* outSize) {
  Eigen::MatrixXd matV;
  Eigen::MatrixXi matF;
  igl::read_triangle_mesh(filename, matV, matF);

  auto mesh = GeoSharPlusCPP::Mesh(matV, matF);

  // Serialize the mesh into the allocated buffer
  if (!GS::serializeMesh(mesh, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_write_triangle_mesh(
    const uint8_t* inBuffer, const int inSize, const char* filename) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  if (!igl::write_triangle_mesh(filename, mesh.V, mesh.F)) {
    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_centroid(const uint8_t* inBuffer,
                                                   int inSize,
                                                   uint8_t** outBuffer,
                                                   int* outSize) {
  *outBuffer = nullptr;
  *outSize = 0;
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  Eigen::Vector3d cen;
  igl::centroid(mesh.V, mesh.F, cen);

  // Serialize the centroid into the allocated buffer
  if (!GS::serializePoint(cen, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;
    return false;
  }
  return true;
}
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_barycenter(const uint8_t* inBuffer,
                                                     int inSize,
                                                     uint8_t** outBuffer,
                                                     int* outSize) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  Eigen::MatrixXd BC;
  igl::barycenter(mesh.V, mesh.F, BC);

  // Serialize the point array into the allocated buffer
  *outBuffer = nullptr;
  *outSize = 0;

  if (!GS::serializePointArray(BC, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_vert_normals(const uint8_t* inBuffer,
                                                       int inSize,
                                                       uint8_t** outBuffer,
                                                       int* outSize) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  Eigen::MatrixXd VN;
  igl::per_vertex_normals(mesh.V, mesh.F, VN);

  // Serialize the point array into the allocated buffer
  *outBuffer = nullptr;
  *outSize = 0;

  // Using PointArray serialization for normals
  if (!GS::serializePointArray(VN, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_face_normals(const uint8_t* inBuffer,
                                                       int inSize,
                                                       uint8_t** outBuffer,
                                                       int* outSize) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  Eigen::MatrixXd FN;
  igl::per_face_normals(mesh.V, mesh.F, FN);

  // Serialize the point array into the allocated buffer
  *outBuffer = nullptr;
  *outSize = 0;

  // Using PointArray serialization for normals
  if (!GS::serializePointArray(FN, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL
IGM_corner_normals(const uint8_t* inBuffer, int inSize, double threshold_deg,
                   uint8_t** outBuffer, int* outSize) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  Eigen::MatrixXd CN;
  igl::per_corner_normals(mesh.V, mesh.F, threshold_deg, CN);

  // Serialize the point array into the allocated buffer
  *outBuffer = nullptr;
  *outSize = 0;

  // Using PointArray serialization for normals
  if (!GS::serializePointArray(CN, *outBuffer, *outSize)) {
    if (*outBuffer) delete[] *outBuffer;  // Cleanup
    *outBuffer = nullptr;
    *outSize = 0;

    return false;
  }

  return true;
}

GEOSHARPLUS_API bool GEOSHARPLUS_CALL
IGM_edge_normals(const uint8_t* inBuffer, int inSize, int weightingType,
                 uint8_t** outBufferA, int* outSizeA, uint8_t** outBufferB,
                 int* outSizeB, uint8_t** outBufferC, int* outSizeC) {
  GeoSharPlusCPP::Mesh mesh;
  if (!GS::deserializeMesh(inBuffer, inSize, mesh)) {
    return false;
  }

  // Calling igl function to compute edge normals
  Eigen::MatrixXd EN;
  Eigen::Matrix<int, Eigen::Dynamic, 2> EI;
  Eigen::VectorXi EMAP;

  igl::per_edge_normals(
      mesh.V, mesh.F,
      static_cast<igl::PerEdgeNormalsWeightingType>(weightingType), EN, EI,
      EMAP);

  // Using PointArray serialization for normals
  *outBufferA = nullptr;
  *outSizeA = 0;
  if (!GS::serializePointArray(EN, *outBufferA, *outSizeA)) {
    if (*outBufferA) delete[] *outBufferA;  // Cleanup
    *outBufferA = nullptr;
    *outSizeA = 0;

    return false;
  }

  // Using PairIntArray serialization for normals
  *outBufferB = nullptr;
  *outSizeB = 0;
  if (!GS::serializeNumberPairArray(EI, *outBufferB, *outSizeB)) {
    if (*outBufferB) delete[] *outBufferB;  // Cleanup
    *outBufferB = nullptr;
    *outSizeB = 0;

    return false;
  }

  // Using NumberArray serialization for normals
  *outBufferC = nullptr;
  *outSizeC = 0;
  if (!GS::serializeNumberArray(EMAP, *outBufferC, *outSizeC)) {
    if (*outBufferC) delete[] *outBufferC;  // Cleanup
    *outBufferC = nullptr;
    *outSizeC = 0;

    return false;
  }
}
}  // namespace GeoSharPlusCPP::Serialization
