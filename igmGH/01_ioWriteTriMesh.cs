using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_write_triangle_mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_write_triangle_mesh()
          : base("Save TriMesh", "igSaveMsh",
              "Save a triangle mesh directly to disk. format supported: obj, off, stl, wrl, ply, mesh.",
              "IG-Mesh", "01|IO+Info")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to save.", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "path", "File path to save the mesh to disk.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Try Save", "T", "Save when TRUE.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Success", "S", "If mesh is saved successfully.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            if (!DA.GetData(0, ref mesh)) { return; }
            if (!mesh.IsValid) { return; }

            string fName = "";
            if (!DA.GetData(1, ref fName)) { return; }

            bool doIt = false;
            if (!DA.GetData(2, ref doIt)) { return; }

            if (doIt)
            {
                IGMRhinoCommon.Utils.SaveMesh(ref mesh, fName);
                DA.SetData(0, "Succeed.");
            }
            else
            {
                // output
                DA.SetData(0, "Failed.");
            }


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
                return Properties.Resources.ioWriteTriMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fcaceca2-c358-4fc0-8b48-f3d31e861bca"); }
        }
    }
}
