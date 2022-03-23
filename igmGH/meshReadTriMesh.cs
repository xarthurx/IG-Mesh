using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_read_triangle_mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_read_triangle_mesh()
          : base("Load TriMesh", "ireadMsh",
              "Read a triangle mesh directly from disk. format supported: obj, off, stl, wrl, ply, mesh.",
              "IGM", "01 | IO+Info")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Location", "file", "Mesh file location from disk.", GH_ParamAccess.item);
            //pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh geometry if loaded successfully.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string fName = "";
            if (!DA.GetData(0, ref fName)) { return; }

            //Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            Rhino.Geometry.Mesh mesh;
            IGLRhinoCommon.Utils.getMesh(fName, out mesh);

            if (!mesh.IsValid) { return; }
            // output
            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0b92e0c5-64a1-4edb-97dc-c8df9f9b088c"); }
        }
    }
}
