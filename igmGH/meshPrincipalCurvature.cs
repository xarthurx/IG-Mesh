using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igmGH
{
    public class IGM_principal_curvature : GH_Component
    {
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGM_principal_curvature()
          : base("Principal Curvature", "iPrincipalCurvature",
              "Compute the principal curvature directions and magnitude of the given triangle mesh.",
              "IGM", "07 | Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("radius", "r", "controls the size of the neighbourhood used, 1 = average edge length.", GH_ParamAccess.item, 5);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Principal Dir 1", "PD1", "Maximal curvature direction for each vertex.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Principal Dir 2", "PD2", "Minimal curvature direction for each vertex.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Principal Value 1", "PV1", "Maximal curvature value for each vertex.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Principal Value 2", "PV2", "Minimal curvature value for each vertex.", GH_ParamAccess.list);
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

            // use default threshold if not given by the user
            double r = 5;
            if (!DA.GetData(1, ref r)) { }

            // call the cpp function to solve the adjacency list
            var (PD1, PD2, PV1, PV2) = IGLRhinoCommon.Utils.getPrincipalCurvature(ref mesh, r);

            // output
            DA.SetDataList(0, PD1);
            DA.SetDataList(1, PD2);
            DA.SetDataList(2, PV1);
            DA.SetDataList(3, PV2);
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
            get { return new Guid("ab67cd8c-b15b-40b4-b052-e8d39ccc1ea5"); }
        }
    }
}
