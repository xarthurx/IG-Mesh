#include "GeoSharPlusCPP/Core/Geometry.h"

namespace GeoSharPlusCPP {  // Corrected namespace name to match the header file

// Polyline operations
double Polyline::length() const {
  double total = 0.0;
  const auto n = vertices.rows();

  for (Eigen::Index i = 1; i < n; ++i) {
    total += (vertices.row(i) - vertices.row(i - 1)).norm();
  }

  return total;
}

// Mesh validation implementation
bool Mesh::validate() const {
  const auto n_V = V.rows();

  // Check face indices are within valid range
  return F.maxCoeff() < n_V && F.minCoeff() >= 0 &&
         (F.cols() == 3 || F.cols() == 4);  // triangles or quads
}

// Add this method to the Mesh class implementation
Eigen::Vector3d Mesh::centroid() const {
  if (V.rows() == 0) {
    return Vector3d::Zero();
  }

  // For closed meshes, use weighted approach
  if (F.rows() > 0) {
    Vector3d center = Vector3d::Zero();
    double totalArea = 0.0;

    for (Eigen::Index i = 0; i < F.rows(); ++i) {
      Vector3d v1 = V.row(F(i, 0));
      Vector3d v2 = V.row(F(i, 1));
      Vector3d v3 = V.row(F(i, 2));

      double area = 0.5 * (v2 - v1).cross(v3 - v1).norm();
      Vector3d triangleCenter = (v1 + v2 + v3) / 3.0;

      center += area * triangleCenter;
      totalArea += area;
    }

    if (totalArea > 0) {
      center /= totalArea;
    }

    return center;
  }

  // Simple average of vertices for non-closed meshes
  return V.colwise().mean();
}

// Bounding box calculation for mesh
std::pair<Vector3d, Vector3d> Mesh::boundingBox() const {
  if (V.rows() == 0) {  // Corrected to use V instead of vertices
    return {Vector3d::Zero(), Vector3d::Zero()};
  }

  Vector3d min =
      V.colwise().minCoeff();  // Corrected to use V instead of vertices
  Vector3d max =
      V.colwise().maxCoeff();  // Corrected to use V instead of vertices
  return {min, max};
}
}  // namespace GeoSharPlusCPP
