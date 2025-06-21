using Google.FlatBuffers;
using Rhino.Geometry;

namespace GSP
{
  public static class Wrapper
  {
    // Point3d IO
    public static byte[] ToPointBuffer(Point3d pt)
    {
      var builder = new FlatBufferBuilder(64); // Enough for a single Vector3d

      // Create a Vector3d in the FlatBuffer
      FB.PointData.StartPointData(builder);
      var vecOffset = FB.Vec3.CreateVec3(builder, pt.X, pt.Y, pt.Z);
      FB.PointData.AddPoint(builder, vecOffset);
      var ptOffset = FB.PointData.EndPointData(builder);

      // Finish the buffer with the root table offset
      builder.Finish(ptOffset.Value);

      // Now get the completed buffer
      return builder.SizedByteArray();
    }

    public static Point3d FromPointBuffer(byte[] buffer)
    {
      var ptByteBuffer = new ByteBuffer(buffer);
      var pt = FB.PointData.GetRootAsPointData(ptByteBuffer).Point;

      return pt.HasValue ? new Point3d(pt.Value.X, pt.Value.Y, pt.Value.Z)
        : new Point3d(0, 0, 0); // Default value if null
    }

    // Point3dArray IO
    public static byte[] ToPointArrayBuffer(Point3d[] points)
    {
      var builder = new FlatBufferBuilder(1024);

      // Add points in reverse order (FlatBuffers build vectors backwards) to build the point vector
      FB.PointArrayData.StartPointsVector(builder, points.Length);
      for (int i = points.Length - 1; i >= 0; i--)
      {
        FB.Vec3.CreateVec3(builder, points[i].X, points[i].Y, points[i].Z);
      }
      var ptOffset = builder.EndVector();

      var arrayOffset = FB.PointArrayData.CreatePointArrayData(builder, ptOffset);
      builder.Finish(arrayOffset.Value);

      return builder.SizedByteArray();
    }

    public static Point3d[] FromPointArrayBuffer(byte[] buffer)
    {
      var byteBuffer = new ByteBuffer(buffer);
      var pointArray = FB.PointArrayData.GetRootAsPointArrayData(byteBuffer);

      var pts = pointArray.Points;
      // 1. Check if the array is valid
      if (pointArray.PointsLength == 0)
        return Array.Empty<Point3d>();

      // 2. Extract structs directly
      var res = new Point3d[pointArray.PointsLength];
      for (int i = 0; i < pointArray.PointsLength; i++)
      {
        var pt = pointArray.Points(i);
        res[i] = pt.HasValue ? new Point3d(pt.Value.X, pt.Value.Y, pt.Value.Z) : new Point3d(0, 0, 0); // Default value if null
      }
      return res;
    }
    // Mesh IO
    public static byte[] ToMeshBuffer(Mesh mesh)
    {
      var builder = new FlatBufferBuilder(1024);

      // Triangulate the mesh if it's not already triangulated
      Mesh triangulatedMesh = mesh.DuplicateMesh();
      if (!triangulatedMesh.Faces.TriangleCount.Equals(triangulatedMesh.Faces.Count))
      {
        triangulatedMesh.Faces.ConvertQuadsToTriangles();
      }

      // Add vertices
      FB.MeshData.StartVerticesVector(builder, triangulatedMesh.Vertices.Count);
      for (int i = triangulatedMesh.Vertices.Count - 1; i >= 0; i--)
      {
        var vertex = triangulatedMesh.Vertices[i];
        FB.Vec3.CreateVec3(builder, vertex.X, vertex.Y, vertex.Z);
      }
      var verticesOffset = builder.EndVector();

      // Add faces (triangles)
      FB.MeshData.StartFacesVector(builder, triangulatedMesh.Faces.Count);
      for (int i = triangulatedMesh.Faces.Count - 1; i >= 0; i--)
      {
        var face = triangulatedMesh.Faces[i];
        FB.Vec3i.CreateVec3i(builder, face.A, face.B, face.C); // Using Vec3 to store three integers
      }
      var facesOffset = builder.EndVector();

      // Create the mesh data
      var meshOffset = FB.MeshData.CreateMeshData(builder, verticesOffset, facesOffset);
      builder.Finish(meshOffset.Value);

      return builder.SizedByteArray();
    }

    public static Mesh FromMeshBuffer(byte[] buffer)
    {
      var byteBuffer = new ByteBuffer(buffer);
      var meshData = FB.MeshData.GetRootAsMeshData(byteBuffer);

      var mesh = new Mesh();

      // Add vertices
      for (int i = 0; i < meshData.VerticesLength; i++)
      {
        var vertex = meshData.Vertices(i);
        if (vertex.HasValue)
        {
          mesh.Vertices.Add(vertex.Value.X, vertex.Value.Y, vertex.Value.Z);
        }
      }

      // Add faces
      for (int i = 0; i < meshData.FacesLength; i++)
      {
        var face = meshData.Faces(i);
        if (face.HasValue)
        {
          mesh.Faces.AddFace(face.Value.X, face.Value.Y, face.Value.Z);
        }
      }

      if (mesh.IsValid)
      {
        mesh.RebuildNormals();
        mesh.Compact();
      }

      return mesh;
    }

    // Mesh IO

  }
}
