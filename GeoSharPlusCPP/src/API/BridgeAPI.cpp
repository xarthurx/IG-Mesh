#include "GeoSharPlusCPP/API/BridgeAPI.h"

#include <igl/barycenter.h>
#include <igl/centroid.h>
#include <igl/read_triangle_mesh.h>
#include <igl/write_triangle_mesh.h>

#include <iostream>
#include <memory>

#include "GSP_FB/cpp/mesh_generated.h"
#include "GSP_FB/cpp/pointArray_generated.h"
#include "GSP_FB/cpp/point_generated.h"
#include "GeoSharPlusCPP/Core/MathTypes.h"
#include "GeoSharPlusCPP/Serialization/GeoSerializer.h"

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
}  // namespace GeoSharPlusCPP::Serialization
