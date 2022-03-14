using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace igl_GrassHopper
{
    public class IGL_laplacian : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGL_laplacian()
          : base("IGL_Laplacian", "iLaplacian",
              "Solve laplacian equation under given boundary condition.",
              "IGL+", "mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Constraint Indices", "I", "the indices to be constrained", GH_ParamAccess.list);
            pManager.AddNumberParameter("Constraint Values", "V", "the values to constrain with", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddMeshParameter("Mesh", "M", "output mesh with color info.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scalar Value", "D", "scalar value for all vertices.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            List<int> con_idx = new List<int>();
            List<float> con_val = new List<float>();

            if (!DA.GetData(0, ref mesh)) { return; }
            if (!mesh.IsValid) { return; }

            if (!DA.GetDataList(1, con_idx)) { return; }
            if (!DA.GetDataList(2, con_val)) { return; }
            if (!(con_idx.Count > 0) || !(con_val.Count > 0)) { return; }
            if (con_idx.Count != con_val.Count) { return; }


            // call the cpp function to solve the adjacency list
            var res = IGLRhinoCommon.Utils.getLapacianScalar(ref mesh, ref con_idx, ref con_val);


            DA.SetDataList(0, res);
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
            get { return new Guid("9a5af6ef-c8fd-4e0f-9e70-84b709f53be7"); }
        }
    }
}