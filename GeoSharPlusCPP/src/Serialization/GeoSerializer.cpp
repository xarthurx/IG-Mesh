#include "GeoSharPlusCPP/Serialization/GeoSerializer.h"

#include <combaseapi.h>  // Add this include for CoTaskMemAlloc

#include "GSP_FB/cpp/mesh_generated.h"
#include "GSP_FB/cpp/pointArray_generated.h"
#include "GSP_FB/cpp/point_generated.h"
#include "GeoSharPlusCPP/Core/MathTypes.h"
#include "flatbuffers/flatbuffers.h"

namespace GeoSharPlusCPP::Serialization {
bool serializePoint(const Vector3d& point, uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  auto vec = GSP::FB::Vec3(point[0], point[1], point[2]);
  auto ptOffset = GSP::FB::CreatePointData(builder, &vec);
  builder.Finish(ptOffset);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(CoTaskMemAlloc(resSize));
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

  point = Vector3d(ptData->point()->x(), ptData->point()->y(),
                   ptData->point()->z());
  return true;
}

bool serializePointArray(const std::vector<Vector3d>& points,
                         uint8_t*& resBuffer, int& resSize) {
  flatbuffers::FlatBufferBuilder builder;

  // Convert Eigen data to FlatBuffer vectors
  std::vector<GSP::FB::Vec3> pointVector;
  pointVector.reserve(points.size());
  for (const auto& p : points) {
    pointVector.emplace_back(p.x(), p.y(), p.z());
  }

  auto vecVector = builder.CreateVectorOfStructs(pointVector);
  auto ptArray = GSP::FB::CreatePointArrayData(builder, vecVector);
  builder.Finish(ptArray);

  // Copy the serialized data to the provided buffer
  resSize = builder.GetSize();
  resBuffer = static_cast<uint8_t*>(CoTaskMemAlloc(resSize));
  if (!resBuffer) {
    return false;  // Handle allocation failure
  }
  std::memcpy(resBuffer, builder.GetBufferPointer(), resSize);

  return true;
}

bool deserializePointArray(const uint8_t* data, int size,
                           std::vector<Vector3d>& pointArray) {
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
  // Clear the output vector and reserve space
  pointArray.clear();

  // Convert each FlatBuffers Vec3 to an Eigen Vector3d
  for (size_t i = 0; i < points->size(); i++) {
    auto point = points->Get(i);
    pointArray.emplace_back(point->x(), point->y(), point->z());
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
  resBuffer = static_cast<uint8_t*>(CoTaskMemAlloc(resSize));
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
  for (size_t i = 0; i < vertices->size(); i++) {
    auto vertex = vertices->Get(i);
    mesh.V.row(i) =
        Eigen::Vector3d(vertex->x(), vertex->y(), vertex->z()).transpose();
  }

  // Extract faces
  auto faces = meshData->faces();
  if (!faces) {
    return false;
  }
  mesh.F.resize(faces->size(), 3);
  for (size_t i = 0; i < faces->size(); i++) {
    auto face = faces->Get(i);
    mesh.F.row(i) =
        Eigen::Vector3i(face->x(), face->y(), face->z()).transpose();
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
}  // namespace GeoSharPlusCPP::Serialization
