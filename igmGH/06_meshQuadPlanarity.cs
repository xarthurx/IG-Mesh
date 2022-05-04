using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_quad_planarity : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_quad_planarity()
          : base("Quad Mesh Planarity", "igQuadPlanarity",
              "Compute the planarity of the quad faces in a quad mesh.",
              "IG-Mesh", "06|Util")
        {
        }

        /// <summary>
        /// icon position in a category
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Face Planarity", "FP", "The planarity of the quad faces.", GH_ParamAccess.list);
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
            var P = IGMRhinoCommon.Utils.getQuadPlanarity(ref mesh);

            // output
            DA.SetDataList(0, P);
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
            get { return new Guid("cc0fea73-0311-4a62-a383-a9423a0b314c"); }
        }
    }
}
