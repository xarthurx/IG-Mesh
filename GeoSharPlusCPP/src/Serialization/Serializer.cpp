#include "GeoSharPlusCPP/Serialization/Serializer.h"

#ifdef _WIN32
#include <combaseapi.h>  // Windows: CoTaskMemAlloc for COM interop
#else
#include <cstdlib>  // Unix/macOS: use malloc
#endif

#include "GSP_FB/cpp/doubleArray_generated.h"
#include "GSP_FB/cpp/doublePairArray_generated.h"
#include "GSP_FB/cpp/intArray_generated.h"
#include "GSP_FB/cpp/intNestedArray_generated.h"
#include "GSP_FB/cpp/intPairArray_generated.h"
#include "GSP_FB/cpp/mesh_generated.h"
#include "GSP_FB/cpp/pointArray_generated.h"
#include "GSP_FB/cpp/point_generated.h"
#include "GeoSharPlusCPP/Core/MathTypes.h"
#include "flatbuffers/flatbuffers.h"

namespace GeoSharPlusCPP::Serialization {
// Cross-platform memory allocation for C# interop
// On Windows: Use CoTaskMemAlloc (COM-compatible)
// On Unix/macOS: Use malloc (compatible with P/Invoke Marshal.FreeCoTaskMem)
inline void* AllocateInteropMemory(size_t size) {
#ifdef _WIN32
  return CoTaskMemAlloc(size);
#else
  return malloc(size);
#endif
}

// Helper template to get the element type of a container
template <typename Container>
struct element_type {
  // Default case for std::vector
  using type = typename std::remove_reference_t<Container>::value_type;
};

// Specialization for Eigen::VectorXd
template <>
struct element_type<Eigen::VectorXd> {
  using type = double;
};

// Specialization for Eigen::VectorXi
template <>
struct element_type<Eigen::VectorXi> {
  using type = int;
};

// Specialization for Eigen::MatrixXd
template <>
struct element_type<Eigen::MatrixXd> {
  using type = double;
};

// Helper to get the pair element type
template <typename PairContainer>
struct pair_element_type {
  // Default for vectors of pairs
  using type = typename std::remove_reference_t<PairContainer>::value_type::first_type;
};
// Specialization for Eigen::Matrix<int, Dynamic, 2>
template <>
struct pair_element_type<Eigen::Matrix<int, Eigen::Dynamic, 2>> {
  using type = int;
};

// Specialization for Eigen::Matrix<double, Dynamic, 2>
template <>
struct pair_element_type<Eigen::Matrix<double, Eigen::Dynamic, 2>> {
  using type = double;
};

// Unified number array serialization - detects both container and element types
template <typename NumberContainer>
bool serializeNumberArray(const NumberContainer& numbers, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Extract value type from container
  using ValueType = typename element_type<NumberContainer>::type;

  if constexpr (std::is_same_v<ValueType, double>) {
    // Handle double values
    std::vector<double> valueVector;

    if constexpr (std::is_same_v<NumberContainer, std::vector<double>>) {
      valueVector = numbers;
    } else if constexpr (std::is_same_v<NumberContainer, Eigen::VectorXd>) {
      valueVector = std::vector<double>(numbers.data(), numbers.data() + numbers.size());
    }

    auto valuesVector = builder.CreateVector(valueVector);
    auto arrayOffset = GSP::FB::CreateDoubleArrayData(builder, valuesVector);
    builder.Finish(arrayOffset);
  } else if constexpr (std::is_same_v<ValueType, int>) {
    // Handle integer values
    std::vector<int32_t> valueVector;

    if constexpr (std::is_same_v<NumberContainer, std::vector<int>>) {
      valueVector = numbers;
    } else if constexpr (std::is_same_v<NumberContainer, Eigen::VectorXi>) {
      valueVector = std::vector<int>(numbers.data(), numbers.data() + numbers.size());
    }

    auto valuesVector = builder.CreateVector(valueVector);
    auto arrayOffset = GSP::FB::CreateIntArrayData(builder, valuesVector);
    builder.Finish(arrayOffset);
  }

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

// Unified number array deserialization - detects container and element types
template <typename NumberContainer>
bool deserializeNumberArray(const uint8_t* data, int size, NumberContainer& numberArray) {
  // Extract value type from container
  using ValueType = typename element_type<NumberContainer>::type;

  if constexpr (std::is_same_v<ValueType, double>) {
    // Handle double values
    flatbuffers::Verifier verifier(data, size);
    if (!verifier.VerifyBuffer<GSP::FB::DoubleArrayData>()) {
      return false;
    }

    auto arrayData = GSP::FB::GetDoubleArrayData(data);
    if (!arrayData || !arrayData->values()) {
      return false;
    }

    auto values = arrayData->values();

    if constexpr (std::is_same_v<NumberContainer, std::vector<double>>) {
      numberArray.clear();
      numberArray.reserve(values->size());
      for (size_t i = 0; i < values->size(); i++) {
        numberArray.push_back(values->Get(i));
      }
    } else if constexpr (std::is_same_v<NumberContainer, Eigen::VectorXd>) {
      numberArray.resize(values->size());
      for (size_t i = 0; i < values->size(); i++) {
        numberArray(i) = values->Get(i);
      }
    }
  } else if constexpr (std::is_same_v<ValueType, int>) {
    // Handle integer values
    flatbuffers::Verifier verifier(data, size);
    if (!verifier.VerifyBuffer<GSP::FB::IntArrayData>()) {
      return false;
    }

    auto arrayData = GSP::FB::GetIntArrayData(data);
    if (!arrayData || !arrayData->values()) {
      return false;
    }

    auto values = arrayData->values();

    if constexpr (std::is_same_v<NumberContainer, std::vector<int>>) {
      numberArray.clear();
      numberArray.reserve(values->size());
      for (int i = 0; i < values->size(); i++) {
        numberArray.push_back(values->Get(i));
      }
    } else if constexpr (std::is_same_v<NumberContainer, Eigen::VectorXi>) {
      numberArray.resize(values->size());
      for (int i = 0; i < values->size(); i++) {
        numberArray(i) = values->Get(i);
      }
    }
  }

  return true;
}

// Unified number pair array serialization
template <typename PairContainer>
bool serializeNumberPairArray(const PairContainer& pairs, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Extract element type from container
  using ElementType = typename pair_element_type<PairContainer>::type;

  if constexpr (std::is_same_v<ElementType, int>) {
    // Handle integer pairs
    std::vector<GSP::FB::Vec2i> pairVector;

    if constexpr (std::is_same_v<PairContainer, std::vector<std::pair<int, int>>>) {
      // Handle std::vector<std::pair<int, int>>
      pairVector.reserve(pairs.size());
      for (const auto& p : pairs) {
        pairVector.emplace_back(GSP::FB::Vec2i(p.first, p.second));
      }
    } else if constexpr (std::is_same_v<PairContainer, Eigen::Matrix<int, Eigen::Dynamic, 2>>) {
      // Handle Eigen::Matrix<int, Dynamic, 2>
      pairVector.reserve(pairs.rows());
      for (Eigen::Index i = 0; i < pairs.rows(); ++i) {
        pairVector.emplace_back(GSP::FB::Vec2i(pairs(i, 0), pairs(i, 1)));
      }
    }

    auto pairsVector = builder.CreateVectorOfStructs(pairVector);
    auto arrayOffset = GSP::FB::CreateIntPairArrayData(builder, pairsVector);
    builder.Finish(arrayOffset);
  } else if constexpr (std::is_same_v<ElementType, double>) {
    // Handle double pairs
    std::vector<GSP::FB::Vec2> pairVector;

    if constexpr (std::is_same_v<PairContainer, std::vector<std::pair<double, double>>>) {
      // Handle std::vector<std::pair<double, double>>
      pairVector.reserve(pairs.size());
      for (const auto& p : pairs) {
        pairVector.emplace_back(GSP::FB::Vec2(p.first, p.second));
      }
    } else if constexpr (std::is_same_v<PairContainer, Eigen::Matrix<double, Eigen::Dynamic, 2>>) {
      // Handle Eigen::Matrix<double, Dynamic, 2>
      pairVector.reserve(pairs.rows());
      for (Eigen::Index i = 0; i < pairs.rows(); ++i) {
        pairVector.emplace_back(GSP::FB::Vec2(pairs(i, 0), pairs(i, 1)));
      }
    }

    auto pairsVector = builder.CreateVectorOfStructs(pairVector);
    auto arrayOffset = GSP::FB::CreateDoublePairArrayData(builder, pairsVector);
    builder.Finish(arrayOffset);
  }

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

// Unified number pair array deserialization
template <typename PairContainer>
bool deserializeNumberPairArray(const uint8_t* data, int size, PairContainer& pairArray) {
  // Extract element type from container
  using ElementType = typename pair_element_type<PairContainer>::type;

  if constexpr (std::is_same_v<ElementType, int>) {
    // Handle integer pairs
    flatbuffers::Verifier verifier(data, size);
    if (!verifier.VerifyBuffer<GSP::FB::IntPairArrayData>()) {
      return false;
    }

    auto arrayData = GSP::FB::GetIntPairArrayData(data);
    if (!arrayData || !arrayData->pairs()) {
      return false;
    }

    auto pairs = arrayData->pairs();

    if constexpr (std::is_same_v<PairContainer, std::vector<std::pair<int, int>>>) {
      // Clear the output vector and reserve space
      pairArray.clear();
      pairArray.reserve(pairs->size());

      // Fill with pairs
      for (int i = 0; i < pairs->size(); i++) {
        auto pair = pairs->Get(i);
        pairArray.emplace_back(std::make_pair(pair->x(), pair->y()));
      }
    } else if constexpr (std::is_same_v<PairContainer, Eigen::Matrix<int, Eigen::Dynamic, 2>>) {
      // Resize the matrix
      pairArray.resize(pairs->size(), 2);

      // Fill the matrix
      for (size_t i = 0; i < pairs->size(); i++) {
        auto pair = pairs->Get(i);
        pairArray(i, 0) = pair->x();
        pairArray(i, 1) = pair->y();
      }
    }
  } else if constexpr (std::is_same_v<ElementType, double>) {
    // Handle double pairs
    flatbuffers::Verifier verifier(data, size);
    if (!verifier.VerifyBuffer<GSP::FB::DoublePairArrayData>()) {
      return false;
    }

    auto arrayData = GSP::FB::GetDoublePairArrayData(data);
    if (!arrayData || !arrayData->pairs()) {
      return false;
    }

    auto pairs = arrayData->pairs();

    if constexpr (std::is_same_v<PairContainer, std::vector<std::pair<double, double>>>) {
      // Clear the output vector and reserve space
      pairArray.clear();
      pairArray.reserve(pairs->size());

      // Fill with pairs
      for (size_t i = 0; i < pairs->size(); i++) {
        auto pair = pairs->Get(i);
        pairArray.emplace_back(std::make_pair(pair->x(), pair->y()));
      }
    } else if constexpr (std::is_same_v<PairContainer, Eigen::Matrix<double, Eigen::Dynamic, 2>>) {
      // Resize the matrix
      pairArray.resize(pairs->size(), 2);

      // Fill the matrix
      for (size_t i = 0; i < pairs->size(); i++) {
        auto pair = pairs->Get(i);
        pairArray(i, 0) = pair->x();
        pairArray(i, 1) = pair->y();
      }
    }
  }

  return true;
}

bool serializePoint(const Vector3d& point, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  auto vec = GSP::FB::Vec3(point[0], point[1], point[2]);
  auto ptOffset = GSP::FB::CreatePointData(builder, &vec);
  builder.Finish(ptOffset);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

bool deserializePoint(const uint8_t* buffer, int size, Vector3d& point) {
  flatbuffers::Verifier verifier(buffer, size);
  if (!verifier.VerifyBuffer<GSP::FB::PointData>()) {
    return false;
  }

  auto ptData = GSP::FB::GetPointData(buffer);
  if (!ptData) {
    return false;
  }

  point = Vector3d(ptData->point()->x(), ptData->point()->y(), ptData->point()->z());
  return true;
}

// Template function to handle both vector<Vector3d> and MatrixXd
template <typename PointContainer>
bool serializePointArray(const PointContainer& points, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Convert data to FlatBuffer vectors
  std::vector<GSP::FB::Vec3> pointVector;
  if constexpr (std::is_same_v<PointContainer, std::vector<Vector3d>>) {
    // Handle std::vector<Vector3d>
    pointVector.reserve(points.size());
    for (const auto& p : points) {
      pointVector.emplace_back(p.x(), p.y(), p.z());
    }
  } else if constexpr (std::is_same_v<PointContainer, Eigen::MatrixXd>) {
    // Handle Eigen::MatrixXd
    pointVector.reserve(points.rows());
    for (Eigen::Index i = 0; i < points.rows(); ++i) {
      pointVector.emplace_back(points(i, 0), points(i, 1), points(i, 2));
    }
  }

  auto vecVector = builder.CreateVectorOfStructs(pointVector);
  auto ptArray = GSP::FB::CreatePointArrayData(builder, vecVector);
  builder.Finish(ptArray);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}
// Template function to handle both vector<Vector3d> and MatrixXd output types
template <typename PointContainer>
bool deserializePointArray(const uint8_t* data, int size, PointContainer& pointArray) {
  // Verify the buffer integrity
  flatbuffers::Verifier verifier(data, size);
  if (!verifier.VerifyBuffer<GSP::FB::PointArrayData>()) {
    return false;
  }

  // Get the vector from the buffer
  auto ptArrayData = GSP::FB::GetPointArrayData(data);
  if (!ptArrayData) {
    return false;
  }

  auto points = ptArrayData->points();

  if constexpr (std::is_same_v<PointContainer, std::vector<Vector3d>>) {
    // Clear the output vector and reserve space for better performance
    pointArray.clear();
    pointArray.reserve(points->size());

    // Convert each FlatBuffers Vec3 to an Eigen Vector3d
    for (size_t i = 0; i < points->size(); i++) {
      auto point = points->Get(i);
      pointArray.emplace_back(point->x(), point->y(), point->z());
    }
  } else if constexpr (std::is_same_v<PointContainer, Eigen::MatrixXd>) {
    // Resize the matrix to hold all points
    pointArray.resize(points->size(), 3);

    // Fill the matrix with point data
    for (size_t i = 0; i < points->size(); i++) {
      auto point = points->Get(i);
      pointArray.row(i) << point->x(), point->y(), point->z();
    }
  }

  return true;
}

bool serializeMesh(const Mesh& mesh, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Convert vertices to flatbuffers compatible format
  std::vector<GSP::FB::Vec3> vertices;
  vertices.reserve(mesh.V.rows());
  for (int i = 0; i < mesh.V.rows(); i++) {
    vertices.emplace_back(mesh.V(i, 0), mesh.V(i, 1), mesh.V(i, 2));
  }

  // Convert faces to flatbuffers compatible format
  std::vector<GSP::FB::Vec3i> faces;
  faces.reserve(mesh.F.rows());
  for (int i = 0; i < mesh.F.rows(); i++) {
    faces.emplace_back(mesh.F(i, 0), mesh.F(i, 1), mesh.F(i, 2));
  }

  // Create vectors in flatbuffers
  auto verticesVector = builder.CreateVectorOfStructs(vertices);
  auto facesVector = builder.CreateVectorOfStructs(faces);

  // Create colors vector if present
  flatbuffers::Offset<flatbuffers::Vector<float>> colorsVector;
  bool hasColors = mesh.C.size() > 0;
  if (hasColors) {
    std::vector<float> colors(mesh.C.data(), mesh.C.data() + mesh.C.size());
    colorsVector = builder.CreateVector(colors);
  }

  // Create the mesh
  GSP::FB::MeshDataBuilder meshBuilder(builder);
  meshBuilder.add_vertices(verticesVector);
  meshBuilder.add_faces(facesVector);
  // if (hasColors) {
  //   meshBuilder.add_colors(colorsVector);
  //   meshBuilder.add_has_colors(true);
  // }
  auto meshOffset = meshBuilder.Finish();
  builder.Finish(meshOffset);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

bool deserializeMesh(const uint8_t* data, int size, Mesh& mesh) {
  // Verify the buffer integrity
  flatbuffers::Verifier verifier(data, size);
  if (!verifier.VerifyBuffer<GSP::FB::MeshData>()) {
    return false;
  }

  // Get the mesh data from the buffer
  auto meshData = GSP::FB::GetMeshData(data);
  if (!meshData) {
    return false;
  }

  // Extract vertices
  auto vertices = meshData->vertices();
  if (!vertices) {
    return false;
  }
  mesh.V.resize(vertices->size(), 3);
  for (int i = 0; i < vertices->size(); i++) {
    auto vertex = vertices->Get(i);
    mesh.V.row(i) = Eigen::Vector3d(vertex->x(), vertex->y(), vertex->z()).transpose();
  }

  // Extract faces
  auto faces = meshData->faces();
  if (!faces) {
    return false;
  }
  mesh.F.resize(faces->size(), 3);
  for (size_t i = 0; i < faces->size(); i++) {
    auto face = faces->Get(i);
    mesh.F.row(i) = Eigen::Vector3i(face->x(), face->y(), face->z()).transpose();
  }

  // Extract colors if present
  // if (meshData->has_colors() && meshData->colors()) {
  //  auto colors = meshData->colors();
  //  mesh.C.resize(colors->size());
  //  for (size_t i = 0; i < colors->size(); i++) {
  //    mesh.C(i) = colors->Get(i);
  //  }
  //} else {
  //  mesh.C.resize(0);
  //}

  return true;
}

template bool
serializeNumberArray(const std::vector<double>& numbers, uint8_t*& resBuffer, int& resSize);
template bool
serializeNumberArray(const std::vector<int>& numbers, uint8_t*& resBuffer, int& resSize);
template bool
serializeNumberArray(const Eigen::VectorXd& numbers, uint8_t*& resBuffer, int& resSize);
template bool
serializeNumberArray(const Eigen::VectorXi& numbers, uint8_t*& resBuffer, int& resSize);
template bool
deserializeNumberArray(const uint8_t* data, int size, std::vector<double>& numberArray);
template bool deserializeNumberArray(const uint8_t* data, int size, std::vector<int>& numberArray);
template bool deserializeNumberArray(const uint8_t* data, int size, Eigen::VectorXd& numberArray);
template bool deserializeNumberArray(const uint8_t* data, int size, Eigen::VectorXi& numberArray);
// Explicit instantiations for integer pair arrays
template bool serializeNumberPairArray(const std::vector<std::pair<int, int>>& pairs,
                                       uint8_t*& resBuffer,
                                       int& resSize);
template bool serializeNumberPairArray(const Eigen::Matrix<int, Eigen::Dynamic, 2>& pairs,
                                       uint8_t*& resBuffer,
                                       int& resSize);
template bool deserializeNumberPairArray(const uint8_t* data,
                                         int size,
                                         std::vector<std::pair<int, int>>& pairArray);
template bool deserializeNumberPairArray(const uint8_t* data,
                                         int size,
                                         Eigen::Matrix<int, Eigen::Dynamic, 2>& pairArray);

// Explicit instantiations for double pair arrays
template bool serializeNumberPairArray(const std::vector<std::pair<double, double>>& pairs,
                                       uint8_t*& resBuffer,
                                       int& resSize);
template bool serializeNumberPairArray(const Eigen::Matrix<double, Eigen::Dynamic, 2>& pairs,
                                       uint8_t*& resBuffer,
                                       int& resSize);
template bool deserializeNumberPairArray(const uint8_t* data,
                                         int size,
                                         std::vector<std::pair<double, double>>& pairArray);
template bool deserializeNumberPairArray(const uint8_t* data,
                                         int size,
                                         Eigen::Matrix<double, Eigen::Dynamic, 2>& pairArray);
// Explicit instantiations to ensure the template is compiled for these types
template bool
serializePointArray(const std::vector<Vector3d>& points, uint8_t*& resBuffer, int& resSize);
template bool serializePointArray(const Eigen::MatrixXd& points, uint8_t*& resBuffer, int& resSize);

// Explicit instantiations to ensure the template is compiled for these types
template bool
deserializePointArray(const uint8_t* data, int size, std::vector<Vector3d>& pointArray);
template bool deserializePointArray(const uint8_t* data, int size, Eigen::MatrixXd& pointArray);

// Serialize nested integer arrays (vector<vector<int>>)
bool serializeNestedIntArray(const std::vector<std::vector<int>>& nestedArray,
                             uint8_t*& resBuffer,
                             int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Flatten the nested array and keep track of sizes
  std::vector<int> flatArray;
  std::vector<int> sizes;

  for (const auto& subArray : nestedArray) {
    sizes.push_back(static_cast<int>(subArray.size()));
    for (int value : subArray) {
      flatArray.push_back(value);
    }
  }

  // Create vectors in flatbuffers
  auto valuesVector = builder.CreateVector(flatArray);
  auto sizesVector = builder.CreateVector(sizes);

  // Create the nested array data
  auto nestedArrayOffset = GSP::FB::CreateIntNestedArrayData(builder, valuesVector, sizesVector);
  builder.Finish(nestedArrayOffset);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(AllocateInteropMemory(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

// Deserialize nested integer arrays
bool deserializeNestedIntArray(const uint8_t* data,
                               int size,
                               std::vector<std::vector<int>>& nestedArray) {
  // Verify the buffer integrity
  flatbuffers::Verifier verifier(data, size);
  if (!verifier.VerifyBuffer<GSP::FB::IntNestedArrayData>()) {
    return false;
  }

  // Get the nested array data from the buffer
  auto arrayData = GSP::FB::GetIntNestedArrayData(data);
  if (!arrayData || !arrayData->values() || !arrayData->sizes()) {
    return false;
  }

  auto flatValues = arrayData->values();
  auto sizes = arrayData->sizes();

  // Clear the output vector
  nestedArray.clear();
  nestedArray.reserve(sizes->size());

  // Reconstruct the nested structure
  size_t flatIndex = 0;
  for (size_t i = 0; i < sizes->size(); i++) {
    int subArraySize = sizes->Get(i);
    std::vector<int> subArray;
    subArray.reserve(subArraySize);

    for (int j = 0; j < subArraySize; j++) {
      if (flatIndex < flatValues->size()) {
        subArray.push_back(flatValues->Get(flatIndex++));
      }
    }
    nestedArray.push_back(std::move(subArray));
  }

  return true;
}
}  // namespace GeoSharPlusCPP::Serialization
