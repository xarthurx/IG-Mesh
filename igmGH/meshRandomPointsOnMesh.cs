
using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_random_points_on_mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_random_points_on_mesh()
          : base("Random Points On Mesh", "iRandomPtsMesh",
              "Randomly sample N points on surface of the given mesh.",
              "IGM", "07 | Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "N", "number of sampled points.", GH_ParamAccess.item, 0);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Sampled Points", "P", "The N sampled points.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Face Index", "FI", "The corresponding face indices of the sampled points.", GH_ParamAccess.list);
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

            int N = 1;
            if (!DA.GetData(1, ref N) || N == 0) { return; }

            // call the cpp function to solve the adjacency list
            var (b, fi) = IGLRhinoCommon.Utils.getRandomPointsOnMesh(ref mesh, N);

            // output
            DA.SetDataList(0, b);
            DA.SetDataList(1, fi);
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
            get { return new Guid("5819dc11-ccff-41eb-b126-96c34911ddc1"); }
        }
    }
}
