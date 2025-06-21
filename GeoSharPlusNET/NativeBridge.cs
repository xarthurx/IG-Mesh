//using System;
using System.Runtime.InteropServices;

namespace GSP
{
  public static class NativeBridge
  {
    private const string WinLibName = @"GeoSharPlusCPP.dll";
    private const string MacLibName = @"GeoSharPlusCPP.dylib";

    // For each function, we create 3 functions: Windows, macOS implementations, and the public API

    // Example: Point Round Trip -- Passing a Point3d to C++ and back 
    [DllImport(WinLibName, EntryPoint = "point3d_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dRoundTripWin(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "point3d_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dRoundTripMac(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    public static bool Point3dRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return Point3dRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return Point3dRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }

    // Example: Point Array Round Trip -- Passing an array of Point3d to C++ and back
    [DllImport(WinLibName, EntryPoint = "point3d_array_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dArrayRoundTripWin(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "point3d_array_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Point3dArrayRoundTripMac(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    public static bool Point3dArrayRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return Point3dArrayRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return Point3dArrayRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }
    // Mesh Round Trip -- Passing a Mesh to C++ and back
    [DllImport(WinLibName, EntryPoint = "mesh_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshRoundTripWin(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    [DllImport(MacLibName, EntryPoint = "mesh_roundtrip", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeshRoundTripMac(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize);
    public static bool MeshRoundTrip(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize)
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return MeshRoundTripWin(inBuffer, inSize, out outBuffer, out outSize);
      else
        return MeshRoundTripMac(inBuffer, inSize, out outBuffer, out outSize);
    }



  }
}
