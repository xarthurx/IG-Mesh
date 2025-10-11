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
        return false;
      }
      // Load the mesh using GeoSharPlusCPP
      var success = NativeBridge.LoadMesh(fileName, out IntPtr outBuffer, out int outSize);
      if (!success || outBuffer == IntPtr.Zero) {
        return false;
      }

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      // Convert the pointer to a Rhino Mesh object
      mesh = Wrapper.FromMeshBuffer(byteArray);
      return true;
    }

    public static bool SaveMesh(ref Mesh mesh, string fileName) {
      if (string.IsNullOrEmpty(fileName)) {
        return false;
      }

      var meshBuffer = Wrapper.ToMeshBuffer(mesh);
      var success = NativeBridge.SaveMesh(meshBuffer, meshBuffer.Length, fileName);

      return success;
    }

    public static List<Point3d> GetBarycenter(ref Mesh rMesh) {
      // initialize the pointer and pass data
      if (rMesh == null)
        throw new ArgumentNullException(nameof(rMesh));

      var meshBuffer = Wrapper.ToMeshBuffer(rMesh);
      NativeBridge.IGM_barycenter(meshBuffer, meshBuffer.Length, out IntPtr outBuffer,
                                  out int outSize);

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      var outPt = Wrapper.FromPointArrayBuffer(byteArray).ToList();

      return outPt;
    }

    /// <summary>
    /// Computes per-vertex normals for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <returns>List of vertex normals</returns>
    public static List<Vector3d> GetNormalVert(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));

      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);

      // Call the native function
      var success = NativeBridge.IGM_vert_normals(meshBuffer, meshBuffer.Length,
                                                  out IntPtr outBuffer, out int outSize);

      if (!success || outBuffer == IntPtr.Zero) {
        return new List<Vector3d>();
      }

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      // Deserialize the result
      var normals = Wrapper.FromVector3dArrayBuffer(byteArray).ToList();

      return normals;
    }

    /// <summary>
    /// Computes per-face normals for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <returns>List of face normals</returns>
    public static List<Vector3d> GetNormalFace(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));

      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);

      // Call the native function
      var success = NativeBridge.IGM_face_normals(meshBuffer, meshBuffer.Length,
                                                  out IntPtr outBuffer, out int outSize);

      if (!success || outBuffer == IntPtr.Zero) {
        return new List<Vector3d>();
      }

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      // Deserialize the result
      var normals = Wrapper.FromVector3dArrayBuffer(byteArray).ToList();

      return normals;
    }

    /// <summary>
    /// Computes per-corner normals for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <param name="thresholdDegrees">Angle threshold in degrees for sharp features</param>
    /// <returns>List of lists of corner normals (one list per face)</returns>
    public static List<List<Vector3d>> GetNormalCorner(ref Mesh mesh,
                                                       double thresholdDegrees = 10.0) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));

      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);

      // Call the native function
      var success = NativeBridge.IGM_corner_normals(meshBuffer, meshBuffer.Length, thresholdDegrees,
                                                    out IntPtr outBuffer, out int outSize);

      if (!success || outBuffer == IntPtr.Zero) {
        return new List<List<Vector3d>>();
      }

      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory

      // Deserialize the result - this is expected to be a 2D array
      var points = Wrapper.FromVector3dArrayBuffer(byteArray);

      // Organize into lists per face (assuming 3 corners per face for triangular mesh)
      var cornerNormals = new List<List<Vector3d>>();
      for (int i = 0; i < points.Length; i += 3) {
        var faceCorners = new List<Vector3d>();
        for (int j = 0; j < 3 && (i + j) < points.Length; j++) {
          faceCorners.Add(points[i + j]);
        }
        cornerNormals.Add(faceCorners);
      }

      return cornerNormals;
    }

    /// <summary>
    /// Computes per-edge normals, edge indices, and edge map for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <param name="weightingType">Type of weighting: 0=uniform, 1=area</param>
    /// <returns>Tuple containing edge normals, edge indices, and edge map</returns>
    public static (List<Vector3d> EdgeNormals, List<List<int>> EdgeIndices, List<int> EdgeMap)
        GetNormalEdge(ref Mesh mesh, int weightingType = 1) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));

      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);

      // Call the native function with multiple outputs
      var success = NativeBridge.IGM_edge_normals(
          meshBuffer, meshBuffer.Length, weightingType, out IntPtr enBuffer, out int enSize,
          out IntPtr eiBuffer, out int eiSize, out IntPtr emapBuffer, out int emapSize);

      if (!success) {
        return (new List<Vector3d>(), new List<List<int>>(), new List<int>());
      }

      // Process edge normals
      var enBytes = new byte[enSize];
      Marshal.Copy(enBuffer, enBytes, 0, enSize);
      Marshal.FreeCoTaskMem(enBuffer);
      var edgeNormals = Wrapper.FromVector3dArrayBuffer(enBytes).ToList();

      // Process edge indices
      var eiBytes = new byte[eiSize];
      Marshal.Copy(eiBuffer, eiBytes, 0, eiSize);
      Marshal.FreeCoTaskMem(eiBuffer);
      var indices = Wrapper.FromIntPairArrayBufferToList(eiBytes);

      // Convert to the expected List<List<int>> format
      var edgeIndices = new List<List<int>>();
      foreach (var pair in indices) {
        edgeIndices.Add(new List<int> { pair.Item1, pair.Item2 });
      }

      // Process edge map
      var emapBytes = new byte[emapSize];
      Marshal.Copy(emapBuffer, emapBytes, 0, emapSize);
      Marshal.FreeCoTaskMem(emapBuffer);
      var edgeMap = Wrapper.FromIntArrayBufferToList(emapBytes);

      return (edgeNormals, edgeIndices, edgeMap);
    }

    /// <summary>
    /// Gets the vertex-vertex adjacency list for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <returns>List of lists representing vertex-vertex adjacency</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<List<int>> GetAdjacencyVV(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));
        
      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);
      
      // Call the native function to get vertex-vertex adjacency
      var success = NativeBridge.IGM_vert_vert_adjacency(meshBuffer, meshBuffer.Length,
                                                         out IntPtr outBuffer, out int outSize);
      if (!success || outBuffer == IntPtr.Zero) {
        return new List<List<int>>();
      }
      
      // Copy the result from unmanaged memory to a managed byte array
      var byteArray = new byte[outSize];
      Marshal.Copy(outBuffer, byteArray, 0, outSize);
      Marshal.FreeCoTaskMem(outBuffer);  // Free the unmanaged memory
      
      // Deserialize the result into a list of lists of integers
      var adjacencyList = Wrapper.FromNestedIntArrayBuffer(byteArray);
      return adjacencyList;
    }

    /// <summary>
    /// Gets the vertex-triangle adjacency for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <returns>Tuple containing vertex-triangle adjacency and vertex-triangle indices</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static (List<List<int>> VT, List<List<int>> VTI) GetAdjacencyVT(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));
        
      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);
      
      // Call the native function to get vertex-triangle adjacency
      var success = NativeBridge.IGM_vert_tri_adjacency(meshBuffer, meshBuffer.Length,
                                                       out IntPtr outBufferVT, out int outSizeVT,
                                                       out IntPtr outBufferVTI, out int outSizeVTI);
      if (!success || outBufferVT == IntPtr.Zero || outBufferVTI == IntPtr.Zero) {
        return (new List<List<int>>(), new List<List<int>>());
      }
      
      // Copy the VT result from unmanaged memory to a managed byte array
      var byteArrayVT = new byte[outSizeVT];
      Marshal.Copy(outBufferVT, byteArrayVT, 0, outSizeVT);
      Marshal.FreeCoTaskMem(outBufferVT);  // Free the unmanaged memory
      
      // Copy the VTI result from unmanaged memory to a managed byte array
      var byteArrayVTI = new byte[outSizeVTI];
      Marshal.Copy(outBufferVTI, byteArrayVTI, 0, outSizeVTI);
      Marshal.FreeCoTaskMem(outBufferVTI);  // Free the unmanaged memory
      
      // Deserialize the results into lists of lists of integers
      var vtAdjacency = Wrapper.FromNestedIntArrayBuffer(byteArrayVT);
      var vtiAdjacency = Wrapper.FromNestedIntArrayBuffer(byteArrayVTI);
      
      return (vtAdjacency, vtiAdjacency);
    }

    /// <summary>
    /// Gets the triangle-triangle adjacency for a mesh.
    /// </summary>
    /// <param name="mesh">Input mesh</param>
    /// <returns>Tuple containing triangle-triangle adjacency and triangle-triangle indices</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static (List<List<int>> TT, List<List<int>> TTI) GetAdjacencyTT(ref Mesh mesh) {
      if (mesh == null)
        throw new ArgumentNullException(nameof(mesh));
        
      // Serialize mesh to buffer
      var meshBuffer = Wrapper.ToMeshBuffer(mesh);
      
      // Call the native function to get triangle-triangle adjacency
      var success = NativeBridge.IGM_tri_tri_adjacency(meshBuffer, meshBuffer.Length,
                                                      out IntPtr outBufferTT, out int outSizeTT,
                                                      out IntPtr outBufferTTI, out int outSizeTTI);
      if (!success || outBufferTT == IntPtr.Zero || outBufferTTI == IntPtr.Zero) {
        return (new List<List<int>>(), new List<List<int>>());
      }
      
      // Copy the TT result from unmanaged memory to a managed byte array
      var byteArrayTT = new byte[outSizeTT];
      Marshal.Copy(outBufferTT, byteArrayTT, 0, outSizeTT);
      Marshal.FreeCoTaskMem(outBufferTT);  // Free the unmanaged memory
      
      // Copy the TTI result from unmanaged memory to a managed byte array
      var byteArrayTTI = new byte[outSizeTTI];
      Marshal.Copy(outBufferTTI, byteArrayTTI, 0, outSizeTTI);
      Marshal.FreeCoTaskMem(outBufferTTI);  // Free the unmanaged memory
      
      // Deserialize the results into lists of lists of integers
      var ttAdjacency = Wrapper.FromNestedIntArrayBuffer(byteArrayTT);
      var ttiAdjacency = Wrapper.FromNestedIntArrayBuffer(byteArrayTTI);
      
      return (ttAdjacency, ttiAdjacency);
    }
  }
}
