
using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_mesh_properties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_mesh_properties()
          : base("Mesh Info", "igMeshInfo",
              "Provide various mesh info: V, F, etc.",
              "IG-Mesh", "01 | IO+Info")
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
            pManager.AddPointParameter("Vertex", "V", "The vertices of the input mesh.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Face", "F", "The face of the input mesh as three indices into V.", GH_ParamAccess.tree);
            pManager.AddPointParameter("Centroid", "cen", "The centroid using surface integral if the input mesh is closed.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Volume", "vol", "The volume of the mesh if the input mesh is closed.", GH_ParamAccess.item);
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
            var (V, F, cen, vol) = IGLRhinoCommon.Utils.getMeshInfo(ref mesh);

            Grasshopper.DataTree<int> fTree = new Grasshopper.DataTree<int>();
            for (int i = 0; i < F.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                fTree.AddRange(F[i], path);
            }
            // output
            DA.SetDataList(0, V);
            DA.SetDataTree(1, fTree);
            DA.SetData(2, cen);
            DA.SetData(3, vol);
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
            get { return new Guid("80bb92aa-6cc4-4cd0-bfa8-a841ecbb9da8"); }
        }
    }
}
