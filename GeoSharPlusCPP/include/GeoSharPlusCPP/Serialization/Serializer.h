#pragma once
#include <cstdint>
#include <vector>

#include "GeoSharPlusCPP/Core/Geometry.h"

namespace GeoSharPlusCPP::Serialization {
// ! Basic Type
// Unified number array serialization (handles both double and int)
template <typename NumberContainer>
bool serializeNumberArray(const NumberContainer& numbers, uint8_t*& resBuffer,
                          int& resSize);

template <typename NumberContainer>
bool deserializeNumberArray(const uint8_t* data, int size,
                            NumberContainer& numberArray);
// Index array (pairs of integers) serialization/deserialization
template <typename IndexContainer>
bool serializeNumberPairArray(const IndexContainer& indices,
                              uint8_t*& resBuffer, int& resSize);

template <typename IndexContainer>
bool deserializeNumberPairArray(const uint8_t* data, int size,
                                IndexContainer& indexArray);

// Nested integer array serialization/deserialization
bool serializeNestedIntArray(const std::vector<std::vector<int>>& nestedArray,
                            uint8_t*& resBuffer, int& resSize);

bool deserializeNestedIntArray(const uint8_t* data, int size,
                              std::vector<std::vector<int>>& nestedArray);

// ! Geometry
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
