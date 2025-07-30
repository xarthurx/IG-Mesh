// using System;
using System.Runtime.InteropServices;

namespace GSP {
  public static class NativeBridge {
    private const string WinLibName = @"GeoSharPlusCPP.dll";
    private const string MacLibName = @"GeoSharPlusCPP.dylib";

    // For each function, we create 3 functions: Windows, macOS implementations, and the public API

#region Example Round Trip Functions
    // Double Array Round Trip
    [DllImport(WinLibName, EntryPoint = "double_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoubleArrayRoundTripWin(byte[] inBuffer, int inSize,
                                                       out IntPtr outBuffer, out int outSize);

    [DllImport(MacLibName, EntryPoint = "double_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoubleArrayRoundTripMac(byte[] inBuffer, int inSize,
                                                       out IntPtr outBuffer, out int outSize);

    public static bool DoubleArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                            out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return DoubleArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return DoubleArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Int Array Round Trip
    [DllImport(WinLibName, EntryPoint = "int_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IntArrayRoundTripWin(byte[] inBuffer, int inSize,
                                                    out IntPtr outBuffer, out int outSize);

    [DllImport(MacLibName, EntryPoint = "int_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IntArrayRoundTripMac(byte[] inBuffer, int inSize,
                                                    out IntPtr outBuffer, out int outSize);

    public static bool IntArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                         out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IntArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return IntArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Int Pair Array Round Trip
    [DllImport(WinLibName, EntryPoint = "int_pair_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IntPairArrayRoundTripWin(byte[] inBuffer, int inSize,
                                                        out IntPtr outBuffer, out int outSize);

    [DllImport(MacLibName, EntryPoint = "int_pair_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IntPairArrayRoundTripMac(byte[] inBuffer, int inSize,
                                                        out IntPtr outBuffer, out int outSize);

    public static bool IntPairArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                             out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IntPairArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return IntPairArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Double Pair Array Round Trip
    [DllImport(WinLibName, EntryPoint = "double_pair_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoublePairArrayRoundTripWin(byte[] inBuffer, int inSize,
                                                           out IntPtr outBuffer, out int outSize);

    [DllImport(MacLibName, EntryPoint = "double_pair_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool DoublePairArrayRoundTripMac(byte[] inBuffer, int inSize,
                                                           out IntPtr outBuffer, out int outSize);

    public static bool DoublePairArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                                out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return DoublePairArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return DoublePairArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Example: Point Round Trip -- Passing a Point3d to C++ and back
    [DllImport(WinLibName, EntryPoint = "point3d_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dRoundTripWin(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "point3d_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dRoundTripMac(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);
    public static bool Point3dRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                        out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return Point3dRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return Point3dRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Example: Point Array Round Trip -- Passing an array of Point3d to C++ and back
    [DllImport(WinLibName, EntryPoint = "point3d_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dArrayRoundTripWin(byte[] inBuffer, int inSize,
                                                        out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "point3d_array_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dArrayRoundTripMac(byte[] inBuffer, int inSize,
                                                        out IntPtr outBuffer, out int outSize);
    public static bool Point3dArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                             out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return Point3dArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return Point3dArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }
    // Mesh Round Trip -- Passing a Mesh to C++ and back
    [DllImport(WinLibName, EntryPoint = "mesh_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshRoundTripWin(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                                out int outSize);
    [DllImport(MacLibName, EntryPoint = "mesh_roundtrip",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshRoundTripMac(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                                out int outSize);
    public static bool MeshRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                     out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return MeshRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return MeshRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }
#endregion

//----------------------------------
// IG-MESH native functions
//----------------------------------
#region IG - MESH Functions

    // Mesh Centroid -- calculates the centroid of a mesh
    [DllImport(WinLibName, EntryPoint = "IGM_centroid",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshCentroidWin(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                               out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_centroid",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshCentroidMac(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                               out int outSize);

    public static bool MeshCentroid(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                    out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return MeshCentroidWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return MeshCentroidMac(inBuffer, inSize, out outBuffer, out outSize);
    }
    // Load Mesh -- basic function to get a mesh from the native library
    [DllImport(WinLibName, EntryPoint = "IGM_read_triangle_mesh",
               CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern bool LoadMeshWin(string fileName, out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_read_triangle_mesh",
               CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern bool LoadMeshMac(string fileName, out IntPtr outBuffer, out int outSize);
    public static bool LoadMesh(string fileName, out IntPtr outBuffer, out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return LoadMeshWin(fileName, out outBuffer, out outSize);
      else
        return LoadMeshMac(fileName, out outBuffer, out outSize);
    }

    // Save Mesh -- basic function to export a mesh to local HDD
    [DllImport(WinLibName, EntryPoint = "IGM_write_triangle_mesh",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool SaveMeshWin(byte[] inBuffer, int inSize, string fileName);
    [DllImport(MacLibName, EntryPoint = "IGM_write_triangle_mesh",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool SaveMeshMac(byte[] inBuffer, int inSize, string fileName);
    public static bool SaveMesh(byte[] inBuffer, int inSize, string fileName) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return SaveMeshWin(inBuffer, inSize, fileName);
      else
        return SaveMeshMac(inBuffer, inSize, fileName);
    }

    [DllImport(WinLibName, EntryPoint = "IGM_barycenter",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_barycenterWin(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                                 out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_barycenter",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_barycenterMac(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                                 out int outSize);

    public static bool IGM_barycenter(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                      out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IGM_barycenterWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return IGM_barycenterMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Vertex Normals
    [DllImport(WinLibName, EntryPoint = "IGM_vert_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_vert_normalsWin(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_vert_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_vert_normalsMac(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);

    public static bool IGM_vert_normals(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                        out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IGM_vert_normalsWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return IGM_vert_normalsMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Face Normals
    [DllImport(WinLibName, EntryPoint = "IGM_face_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_face_normalsWin(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_face_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_face_normalsMac(byte[] inBuffer, int inSize,
                                                   out IntPtr outBuffer, out int outSize);

    public static bool IGM_face_normals(byte[] inBuffer, int inSize, out IntPtr outBuffer,
                                        out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IGM_face_normalsWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return IGM_face_normalsMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Corner Normals - note the additional threshold_deg parameter
    [DllImport(WinLibName, EntryPoint = "IGM_corner_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_corner_normalsWin(byte[] inBuffer, int inSize,
                                                     double threshold_deg, out IntPtr outBuffer,
                                                     out int outSize);
    [DllImport(MacLibName, EntryPoint = "IGM_corner_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_corner_normalsMac(byte[] inBuffer, int inSize,
                                                     double threshold_deg, out IntPtr outBuffer,
                                                     out int outSize);

    public static bool IGM_corner_normals(byte[] inBuffer, int inSize, double threshold_deg,
                                          out IntPtr outBuffer, out int outSize) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IGM_corner_normalsWin(inBuffer, inSize, threshold_deg, out outBuffer, out outSize);
      else
        return IGM_corner_normalsMac(inBuffer, inSize, threshold_deg, out outBuffer, out outSize);
    }

    // Edge Normals - note the multiple output parameters
    [DllImport(WinLibName, EntryPoint = "IGM_edge_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_edge_normalsWin(byte[] inBuffer, int inSize, int weightingType,
                                                   out IntPtr obEN, out int obsEN, out IntPtr obEI,
                                                   out int obsEI, out IntPtr obEMAP,
                                                   out int obsEMAP);
    [DllImport(MacLibName, EntryPoint = "IGM_edge_normals",
               CallingConvention = CallingConvention.Cdecl)]
    private static extern bool IGM_edge_normalsMac(byte[] inBuffer, int inSize, int weightingType,
                                                   out IntPtr obEN, out int obsEN, out IntPtr obEI,
                                                   out int obsEI, out IntPtr obEMAP,
                                                   out int obsEMAP);

    public static bool IGM_edge_normals(byte[] inBuffer, int inSize, int weightingType,
                                        out IntPtr obEN, out int obsEN, out IntPtr obEI,
                                        out int obsEI, out IntPtr obEMAP, out int obsEMAP) {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return IGM_edge_normalsWin(inBuffer, inSize, weightingType, out obEN, out obsEN, out obEI,
                                   out obsEI, out obEMAP, out obsEMAP);
      else
        return IGM_edge_normalsMac(inBuffer, inSize, weightingType, out obEN, out obsEN, out obEI,
                                   out obsEI, out obEMAP, out obsEMAP);
    }

#endregion
  }
}
