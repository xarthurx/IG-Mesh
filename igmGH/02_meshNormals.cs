using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igmGH
{
    public class IGM_normals_vert : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_normals_vert()
          : base("Vertex Normal", "igNormals_V",
              "Compute per-vertex normals of the given mesh.",
              "IG-Mesh", "02::Properties")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.secondary;

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
            pManager.AddVectorParameter("Vertex Normals", "VN", "the per-vertex normals of the input mesh's faces", GH_ParamAccess.list);
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
            var vn = IGMRhinoCommon.Utils.GetNormalsVert(ref mesh);

            // output
            DA.SetDataList(0, vn);
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
                return Properties.Resources.meshNormalVertex;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("51a9a99e-207d-4cb7-b49d-f89bab1b446a"); }
        }
    }

    public class IGM_normals_face : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_normals_face()
          : base("Face Normal", "igNormals_F",
              "Compute per-face normals of the given mesh.",
              "IG-Mesh", "02::Properties")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.secondary;

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
            pManager.AddVectorParameter("Face Normals", "FN", "the per-face normals of the input mesh's faces", GH_ParamAccess.list);
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
            var fn = IGMRhinoCommon.Utils.GetNormalsFace(ref mesh);

            // output
            DA.SetDataList(0, fn);
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
                return Properties.Resources.meshNormalFace;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3b03e8b2-ffd3-4679-b94e-3582f485a877"); }
        }
    }

    public class IGL_normals_edge : GH_Component
    {
        /// The igl_edge_normals is a bit messy and unintuitive for the tasks. exclude this file for now.
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGL_normals_edge()
          : base("Edge Normal", "iNormals_E",
              "Compute per-edge normals for a triangle mesh by weighted face normals based on different weighting schemes.",
              "IG-Mesh", "02::Properties")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Weighting Type", "w", "The weighting type how face normals influence edge normals: 0-uniform; 1-area average.", GH_ParamAccess.item, 1);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Edge Normal", "EN", "The per edge normals of the input mesh.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Edge End Index into Vertex List", "EI", "The end point indices from the vertex list.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge Index", "EMAP", "The edge indices from the global edge list.", GH_ParamAccess.list);
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
            var (en, ei, emap) = IGMRhinoCommon.Utils.GetNormalsEdge(ref mesh, w);

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
                return Properties.Resources.meshNormalEdge;
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

    public class IGM_normals_corner : GH_Component
    {
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGM_normals_corner()
          : base("Corner Normal", "igNormals_C",
              "Compute per-corner normals for a triangle mesh by computing the area-weighted average of normals at incident faces whose normals deviate  less than the provided threshold.",
              "IG-Mesh", "02::Properties")
        {
        }

        // icon position in a category
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Degree Threshold", "t", "A threshold in degrees on sharp angles.", GH_ParamAccess.item, 10.0);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Corner Normals", "CN", "The per-corner normals of the input mesh's faces.", GH_ParamAccess.tree);
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
            double t = 10;
            if (!DA.GetData(1, ref t)) { }


            // call the cpp function to solve the adjacency list
            var cn = IGMRhinoCommon.Utils.GetNormalsCorner(ref mesh, t);

            Grasshopper.DataTree<Vector3d> cnTree = new Grasshopper.DataTree<Vector3d>();
            for (int i = 0; i < cn.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                cnTree.AddRange(cn[i], path);
            }
            // output
            DA.SetDataTree(0, cnTree);
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
                return Properties.Resources.meshNormalCorner;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a393245d-7c44-4437-b808-8c375e5e6ec4"); }
        }
    }
}
