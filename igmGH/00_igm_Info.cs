using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace igmGH
{
    public class igmInfo : GH_AssemblyInfo
    {
        public override string Name => "IG-Mesh";
        public override Bitmap Icon => Properties.Resources.meshCurvaturePrincipal;
        public override string Description => "IG-Mesh is a one-stop solution for low-level (vertex-based, edge-based) mesh processing. It transplants many functions available in the computer graphics community to the Rhino+GH platform.";
        public override string AuthorName => "Zhao MA";
        public override string AuthorContact => "https://github.com/xarthurx/IG-Mesh";
        public override string Version => "0.1.5";
        public override Guid Id => new Guid("18bfce35-2f9a-4442-9028-9d0821505dcf");
        public override GH_LibraryLicense License => GH_LibraryLicense.opensource;
    }
}
