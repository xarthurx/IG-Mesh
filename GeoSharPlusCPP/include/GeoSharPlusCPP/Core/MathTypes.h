#pragma once
#include <span>

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

// C++20 span types for safer buffer handling
using ByteSpan = std::span<const uint8_t>;
using MutableByteSpan = std::span<uint8_t>;

// Geometry primitives
struct Point {
  Vector3d position;
};

struct LineSegment {
  Vector3d start;
  Vector3d end;

  [[nodiscard]] double length() const noexcept {
    return (end - start).norm();
  }

  [[nodiscard]] Vector3d midpoint() const noexcept {
    return (start + end) * 0.5;
  }
};
}  // namespace GeoSharPlusCPP
