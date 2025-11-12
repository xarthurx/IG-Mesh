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
  Mesh(const MatrixX3d& vertices, const Eigen::MatrixXi& faces)
      : V(vertices), F(faces) {}

  // Mesh data: V - vertices, F - faces (triangles or quads)
  // F is now dynamic width: 3 columns for triangles, 4 columns for quads
  MatrixX3d V;
  Eigen::MatrixXi F;  // Dynamic width to support both tri and quad meshes

  // Optional per-vertex data
  Eigen::VectorXd C;

  // Helper methods to identify mesh type
  bool isTriangleMesh() const { return F.cols() == 3; }
  bool isQuadMesh() const { return F.cols() == 4; }
  int faceVertexCount() const { return F.cols(); }
  
  bool validate() const;
  Eigen::Vector3d centroid() const;
  std::pair<Vector3d, Vector3d> boundingBox() const;
};
}  // namespace GeoSharPlusCPP
