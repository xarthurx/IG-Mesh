#pragma once
#include <Eigen/Core>
#include <Eigen/Geometry>

namespace GeoSharPlusCPP {
// Basic types compatible with libigl
using Vector3i = Eigen::Vector3i;
using Vector3f = Eigen::Vector3f;
using Vector3d = Eigen::Vector3d;

using MatrixX3f = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
using MatrixX3d = Eigen::Matrix<double, Eigen::Dynamic, 3, Eigen::RowMajor>;
using MatrixX3i = Eigen::Matrix<int, Eigen::Dynamic, 3, Eigen::RowMajor>;
using MatrixX4i = Eigen::Matrix<int, Eigen::Dynamic, 4, Eigen::RowMajor>;  // For quad meshes

// Dynamic width matrix types for flexible mesh support
using MatrixXi = Eigen::Matrix<int, Eigen::Dynamic, Eigen::Dynamic, Eigen::RowMajor>;
using MatrixXd = Eigen::Matrix<double, Eigen::Dynamic, Eigen::Dynamic, Eigen::RowMajor>;

// Geometry primitives
struct Point {
  Vector3d position;
};

struct LineSegment {
  Vector3d start;
  Vector3d end;
};
}  // namespace GeoSharPlusCPP
