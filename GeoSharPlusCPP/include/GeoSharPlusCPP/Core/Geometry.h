#pragma once
#include <vector>

#include "MathTypes.h"

namespace GeoSharPlusCPP {
struct Polyline {
  MatrixX3d vertices;
  [[nodiscard]] double length() const;
};

struct Mesh {
  Mesh() = default;

  // Constructor to initialize mesh with vertices and faces
  Mesh(const MatrixX3d& vertices, const Eigen::MatrixXi& faces) : V(vertices), F(faces) {}

  // Mesh data: V - vertices, F - faces (triangles or quads)
  // F is now dynamic width: 3 columns for triangles, 4 columns for quads
  MatrixX3d V;
  Eigen::MatrixXi F;  // Dynamic width to support both tri and quad meshes

  // Optional per-vertex data
  Eigen::VectorXd C;

  // Helper methods to identify mesh type
  [[nodiscard]] constexpr bool isTriangleMesh() const noexcept {
    return F.cols() == 3;
  }
  [[nodiscard]] constexpr bool isQuadMesh() const noexcept {
    return F.cols() == 4;
  }
  [[nodiscard]] constexpr int faceVertexCount() const noexcept {
    return static_cast<int>(F.cols());
  }

  [[nodiscard]] bool validate() const;
  [[nodiscard]] Eigen::Vector3d centroid() const;
  [[nodiscard]] std::pair<Vector3d, Vector3d> boundingBox() const;
};
}  // namespace GeoSharPlusCPP
