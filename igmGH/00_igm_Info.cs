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
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "A one-stop solution for mesh processing in Rhino+Grasshopper";
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
                return "Dr. Zhao Ma";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "xliotx@gmail.com";
            }
        }
    }
}
