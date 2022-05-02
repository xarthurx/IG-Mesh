using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igmGH
{
    public class IGM_normals_corner : GH_Component
    {
        /// <summary>
        /// Initializes the new instance of the corner_normals class.
        /// </summary>
        public IGM_normals_corner()
          : base("Corner Normal", "igNormals_C",
              "Compute per-corner normals for a triangle mesh by computing the area-weighted average of normals at incident faces whose normals deviate  less than the provided threshold.",
              "IG-Mesh", "02|Properties")
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
            var cn = IGMRhinoCommon.Utils.getNormalsCorner(ref mesh, t);

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
