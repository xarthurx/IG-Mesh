#include "GeoSharPlusCPP/Core/Geometry.h"

namespace GeoSharPlusCPP { // Corrected namespace name to match the header file

// Mesh validation implementation
bool Mesh::validate() const {
  const auto n_V = V.rows();

  // Check face indices are within valid range
  return F.maxCoeff() < n_V && F.minCoeff() >= 0 &&
         (F.cols() == 3 || F.cols() == 4);  // triangles or quads
}

// Polyline operations
double Polyline::length() const {
  double total = 0.0;
  const auto n = vertices.rows();

  for (Eigen::Index i = 1; i < n; ++i) {
    total += (vertices.row(i) - vertices.row(i - 1)).norm();
  }

  return total;
}

// Bounding box calculation for mesh
std::pair<Vector3d, Vector3d> Mesh::boundingBox() const {
  if (V.rows() == 0) { // Corrected to use V instead of vertices
    return {Vector3d::Zero(), Vector3d::Zero()};
  }

  Vector3d min = V.colwise().minCoeff(); // Corrected to use V instead of vertices
  Vector3d max = V.colwise().maxCoeff(); // Corrected to use V instead of vertices
  return {min, max};
}

}  // namespace GeomBridgeCPP
