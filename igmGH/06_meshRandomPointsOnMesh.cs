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
          : base("Random Points On Mesh", "igRndPt",
              "Randomly sample N points on surface of the given mesh with random/uniform distribution.",
              "IG-Mesh", "06 | Util")
        {
        }

        /// <summary>
        /// icon position in a category
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number", "N", "Number of sampled points.", GH_ParamAccess.item, 0);
            // the uniform method uses a blue-noise (Poisson's disk) approach to sample the points.
            pManager.AddIntegerParameter("Method", "M", "The method used for sampling: 0-random; 1-uniform.", GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
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

            int M = 0;
            if (!DA.GetData(2, ref M)) { return; }

            // call the cpp function to solve the adjacency list
            var (p, fi) = IGMRhinoCommon.Utils.getRandomPointsOnMesh(ref mesh, N, M);

            // output
            DA.SetDataList(0, p);
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
                return Properties.Resources.meshRandomPtsOnMesh;
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
