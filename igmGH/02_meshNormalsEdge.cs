using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igmGH 
{
    public class IGL_normals_edge : GH_Component
    {
        /// The igl_edge_normals is a bit messy and unintuitive for the tasks. exclude this file for now.
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGL_normals_edge()
          : base("IGL_NormalsEdge", "iNormals_E",
              "Compute per edge normals for a triangle mesh by weighted face normals based on different weighting schemes.",
              "IG-Mesh", "02 | Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Weighting Type", "w", "The weighting type how face normals influence edge normals: 0-uniform; 1-area average.", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Edge Normal", "EN", "the edge-corner normals of the input mesh's faces.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Edge End Index into Vertex List", "EI", "the end point indices from the vertex list.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge Index", "EMAP", "the edge indices from the edge list.", GH_ParamAccess.list);
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
            int w = 1;
            if (!DA.GetData(1, ref w)) { }
            if (!(w == 0 || w == 1))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "weighting type incorrect.");
                return;
            }

            // call the cpp function to solve the adjacency list
            var (en, ei, emap) = IGMRhinoCommon.Utils.getNormalsEdge(ref mesh, w);

            Grasshopper.DataTree<int> eiTree = new Grasshopper.DataTree<int>();
            for (int i = 0; i < ei.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                eiTree.AddRange(ei[i], path);
            }
            // output
            DA.SetDataList(0, en);
            DA.SetDataTree(1, eiTree);
            DA.SetDataList(2, emap);
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
            get { return new Guid("362d6e4a-81ce-4613-8e71-62019f8aff3d"); }
        }
    }
}
