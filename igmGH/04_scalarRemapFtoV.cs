using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace igmGH
{
    public class IGM_remap_FtoV : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_remap_FtoV()
          : base("Remap Faces To Vertices", "igRemapFV",
              "Move a scalar field defined on faces to vertices by averaging.",
              "IG-Mesh", "04::Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Base mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scalar", "S", "Scalar defined on faces.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Remaped Scalar", "SV", "The remapped valuse on vertices.", GH_ParamAccess.list);
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

            List<double> scalarV = new List<double>();
            if (!DA.GetDataList<double>(1, scalarV)) { return; }
            if (scalarV.Count != mesh.Faces.Count) { return; }

            // call the cpp function to solve the adjacency list
            var sv = IGMRhinoCommon.Utils.RemapFtoV(ref mesh, scalarV);

            // output
            DA.SetDataList(0, sv);
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
                return Properties.Resources.scalarRemapFtoV;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a0ee589c-082d-4ca1-bdfe-f5e3a3ddd6c8"); }
        }
    }
}
