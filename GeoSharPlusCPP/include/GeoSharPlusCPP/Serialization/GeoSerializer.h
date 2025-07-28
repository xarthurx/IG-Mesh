#pragma once
#include <cstdint>
#include <vector>

#include "GeoSharPlusCPP/Core/Geometry.h"

namespace GeoSharPlusCPP::Serialization {
// Point serialization
bool serializePoint(const Vector3d& point, uint8_t*& resBuffer, int& resSize);
bool deserializePoint(const uint8_t* buffer, int size, Vector3d& point);

// Point array (de)serialization
template <typename PointContainer>
bool serializePointArray(const PointContainer& points, uint8_t*& resBuffer,
                         int& resSize);
// Point array deserialization template declaration
template <typename PointContainer>
bool deserializePointArray(const uint8_t* data, int size,
                           PointContainer& pointArray);

// Mesh serialization
bool serializeMesh(const Mesh& mesh, uint8_t*& resBuffer, int& resSize);
bool deserializeMesh(const uint8_t* data, int size, Mesh& mesh);

}  // namespace GeoSharPlusCPP::Serialization
