using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace igmGH
{
    public class igmInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "IG-Mesh";
            }
        }

        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.meshCurvaturePrincipal;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "IG-Mesh is a one-stop solution for low-level (vertex-based, edge-based) mesh processing. It transplants many functions available in the computer graphics community to the Rhino+GH platform.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("18bfce35-2f9a-4442-9028-9d0821505dcf");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Zhao Ma";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "https://github.com/xarthurx/IG-Mesh";
            }
        }

        public override GH_LibraryLicense License
        {
            get
            {
                return GH_LibraryLicense.opensource;
            }

        }

        public override string Version
        {
            get
            {
                return "0.1.5";
            }
        }
    }
}
