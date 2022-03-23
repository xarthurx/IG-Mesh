using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_vert_tri_adjacency : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_vert_tri_adjacency()
          : base("Vertex-Triangle Adjacency", "igAdjVT",
              "Compute the vertex-triangle adjacency relationship of the given mesh.",
              "IGM", "03 | Adjacency+Bound")
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
            pManager.AddIntegerParameter("Adjacency V-T", "VT", "indices of the the ajacent triangles to the corresponding vertex.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Adjacency V-T", "VTI", "index of incidence within incident faces listed in VF.", GH_ParamAccess.tree);
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
            var (vt, vi) = IGLRhinoCommon.Utils.getAdjacencyVT(ref mesh);

            Grasshopper.DataTree<int> adjVT = new Grasshopper.DataTree<int>();
            Grasshopper.DataTree<int> adjVTI = new Grasshopper.DataTree<int>();
            for (int i = 0; i < vt.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                adjVT.AddRange(vt[i], path);
                adjVTI.AddRange(vi[i], path);
            }
            // output
            DA.SetDataTree(0, adjVT);
            DA.SetDataTree(1, adjVTI);
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
            get { return new Guid("32c63b12-c43f-40fb-913c-9df607e43305"); }
        }
    }
}
