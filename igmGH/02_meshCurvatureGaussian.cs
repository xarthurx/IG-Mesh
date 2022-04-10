using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_gaussian_curvature : GH_Component
    {
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGM_gaussian_curvature()
          : base("Gaussian Curvature", "iGaussianCurvature",
              "Compute integral of gaussian curvature of the given mesh.",
              "IG-Mesh", "02 | Properties")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Curvature", "GK", "Gaussian curvature values.", GH_ParamAccess.list);
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

            // call the cpp function to solve the adjacency list
            var k = IGMRhinoCommon.Utils.getGaussianCurvature(ref mesh);

            // output
            DA.SetDataList(0, k);
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
            get { return new Guid("6d32480e-e790-4b3f-9ad5-1fb638b75ba4"); }
        }
    }
}
