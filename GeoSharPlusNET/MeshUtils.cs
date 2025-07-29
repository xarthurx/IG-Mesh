using GSP;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GeoSharpNET {
  public static class MeshUtils {
    private static Point3d Centroid(Mesh mesh) {
      // Serialize the mesh for calling into GeoSharPlusCPP
      byte[] meshBuffer;
      meshBuffer = GSP.Wrapper.ToMeshBuffer(mesh);

      // Call the C++ function to calculate centroid
      var success = NativeBridge.MeshCentroid(meshBuffer, meshBuffer.Length, out IntPtr outBuffer,
                                              out int outSize);
      if (!success) {
        throw new InvalidOperationException("Failed to calculate mesh centroid in native code.");
      }

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      var outPt = Wrapper.FromPointBuffer(byteArray);

      return outPt;
    }

    public static (List<Point3d>, List<List<int>>, Point3d, double) GetMeshInfo(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));

      // Get vertices and faces directly from the mesh
      var V = new List<Point3d>(mesh.Vertices.ToPoint3dArray());
      List<List<int>> F = new List<List<int>>();
      foreach (var f in mesh.Faces) {
        // Check if quad mesh or not
        if (f.IsTriangle)
          F.Add(new List<int> { f.A, f.B, f.C });
        else
          F.Add(new List<int> { f.A, f.B, f.C, f.D });
      }

      // Calculate centroid using our C++ library
      Point3d cen = Centroid(mesh);

      // Use Rhino's native method for volume
      double vol = mesh.Volume();

      return (V, F, cen, vol);
    }

    public static bool LoadMesh(string fileName, out Mesh mesh) {
      mesh = new Mesh();
      if (string.IsNullOrEmpty(fileName)) {
        throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        return false;
      }
      // Load the mesh using GeoSharPlusCPP
      var success = NativeBridge.LoadMesh(fileName, out IntPtr meshPtr);
      if (!success || meshPtr == IntPtr.Zero) {
        throw new InvalidOperationException($"Failed to load mesh from {fileName}.");
        return false;
      }
      // Convert the pointer to a Rhino Mesh object
      mesh = Wrapper.FromMeshBuffer(meshPtr);
      return true;
    }
  }
}
