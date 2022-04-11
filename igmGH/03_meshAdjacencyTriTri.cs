using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_tri_tri_adjacency : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_tri_tri_adjacency()
          : base("Triangle-Triangle Adjacency", "igAdjTT",
              "Compute the triangle-triangle adjacency relationship of the given mesh.",
              "IG-Mesh", "03 | Adjacency+Bound")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
            //pManager.AddPointParameter("Mesh V", "V", "A list of mesh vertices.", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Mesh F", "F", "A list of mesh faces.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Adjacency T-T index", "TT", "The item {i}(j) is the id of the triangle adjacent to the j edge of triangle i.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Adjacency T-T edge index", "TTI", "The item{i}(j) is the id of edge of the triangle TT(i,j) that is adjacent with triangle i.", GH_ParamAccess.tree);
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
            var (tt, tti) = IGMRhinoCommon.Utils.getAdjacencyTT(ref mesh);

            Grasshopper.DataTree<int> adjTT = new Grasshopper.DataTree<int>();
            Grasshopper.DataTree<int> adjTTI = new Grasshopper.DataTree<int>();
            for (int i = 0; i < tt.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                adjTT.AddRange(tt[i], path);
                adjTTI.AddRange(tti[i], path);
            }
            // output
            DA.SetDataTree(0, adjTT);
            DA.SetDataTree(1, adjTTI);
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
                return Properties.Resources.meshAdjacencyTriTri;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f2947234-060d-44de-bd8f-f9601b0e780f"); }
        }
    }
}
