using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace igmGH
{
    public class IGM_remap_VtoF : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_remap_VtoF()
          : base("Remap Vertices To Faces", "igRemapVF",
              "Move a scalar field defined on vertices to faces by averaging.",
              "IG-Mesh", "04|Mapping")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Base mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scalar", "S", "Scalar defined on vertices.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Remaped Scalar", "SF", "The remapped valuse on faces.", GH_ParamAccess.list);
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

            List<double> scalarF = new List<double>();
            if (!DA.GetDataList<double>(1, scalarF)) { return; }
            if (scalarF.Count != mesh.Vertices.Count) { return; }

            // call the cpp function to solve the adjacency list
            var sf = IGMRhinoCommon.Utils.RemapVtoF(ref mesh, scalarF);

            // output
            DA.SetDataList(0, sf);
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
                return Properties.Resources.scalarRemapVtoF;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b2743097-03cb-4897-968f-bdcc3e0f95fc"); }
        }
    }
}
