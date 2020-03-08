using System;
using System.Runtime.InteropServices;
//using Rhino.Geometry;

namespace IGLRhinoCommon
{
    internal static class Import
    {
        public const string lib = @"C:\Users\xarthur\source\repos\gh-igl\x64\Debug\igl_cppPort.dll";
    }

    internal static class CppIGL
    {
        /// <summary>
        /// Sums two numbers
        /// </summary>
        [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern double Add(double a, double b);

        /// <summary>
        /// Compute mesh adjacency list
        /// </summary>
        [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int igl_adjacency_list(IntPtr F, int nF, IntPtr adjLstFromCpp, out int sz);

    }
}
