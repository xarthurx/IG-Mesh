using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igl_Grasshopper
{
    public class IGL_BoundEdge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IGL_Barycenter class.
        /// </summary>
        public IGL_BoundEdge()
          : base("IGL_BoundEdge", "iBoundEdge",
              "compute the boundary edges the given mesh.",
              "IGL+", "Boundary")
        {
        }

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
            pManager.AddLineParameter("Boundary edges.", "E", "the boundary edges.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Vertex Indices of the boundary edges.", "EV", "the vertex indices of the boundary edges.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Boundary Triangle indices.", "T", "the indices of the boundary triangles.", GH_ParamAccess.list);
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

            // call the cpp function 
            var (bndE_geo, bndE, bndTi) = IGLRhinoCommon.Utils.getBoundaryEdge(ref mesh);

            // construct the edge tree from the list
            Grasshopper.DataTree<int> evArray = new Grasshopper.DataTree<int>();

            for (int i = 0; i < bndE.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                evArray.AddRange(bndE[i], path);
            }

            // assign to the output
            DA.SetDataList(0, bndE_geo);
            DA.SetDataTree(1, evArray);
            DA.SetDataList(2, bndTi);
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
            get { return new Guid("f4983646-592d-475a-bce2-f57767095810"); }
        }
    }
}
