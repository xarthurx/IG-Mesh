#pragma once
#include <vector>

#include "MathTypes.h"

namespace GeoSharPlusCPP {
struct Polyline {
  MatrixX3d vertices;
  double length() const;
};

struct Mesh {
  Mesh() = default;

  // Constructor to initialize mesh with vertices and faces
  Mesh(const MatrixX3d& vertices, const MatrixX3i& faces)
      : V(vertices), F(faces) {}

  // Mesh data: V - vertices, F - faces (triangles or quads)
  MatrixX3d V;
  MatrixX3i F;

  // Optional per-vertex data
  Eigen::VectorXd C;

  bool validate() const;
  Eigen::Vector3d centroid() const;
  std::pair<Vector3d, Vector3d> boundingBox() const;
};
}  // namespace GeoSharPlusCPP
