#pragma once
#include <vector>

#include "MathTypes.h"

namespace GeoSharPlusCPP {
struct Polyline {
  MatrixX3d vertices;
  double length() const;
};

struct Mesh {
  // Mesh data: V - vertices, F - faces (triangles or quads)
  MatrixX3d V;
  MatrixX3i F;

  // Optional per-vertex data
  Eigen::VectorXd C;

  bool validate() const;
  std::pair<Vector3d, Vector3d> boundingBox() const;
};
}  // namespace GeomBridgeCPP
