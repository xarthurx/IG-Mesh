using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace igl_GrassHopper
{
    public class IGL_barycenter: GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGL_barycenter()
          : base("IGL_Barycenter", "iBarycenter",
              "compute the barycenter of each triangle of the given mesh.",
              "IGL+", "mesh")
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
            pManager.AddIntegerParameter("Boundary Idices", "B", "the boundary list of the input mesh", GH_ParamAccess.tree);
            pManager.AddPointParameter("Boundary Vertices", "P", "the boundary vertices of the input mesh", GH_ParamAccess.tree);
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
            var res = IGLRhinoCommon.Utils.getBoundaryLoop(ref mesh);

            // construct the index & pt tree from the adjacency list
            Grasshopper.DataTree<int> treeArray = new Grasshopper.DataTree<int>();
            Grasshopper.DataTree<Point3d> ptArray = new Grasshopper.DataTree<Point3d>();
            for (int i = 0; i < res.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                treeArray.AddRange(res[i], path);

                foreach (var id in res[i])
                {
                    ptArray.Add(mesh.Vertices[id], path);
                }
            }

            // assign to the output
            DA.SetDataTree(0, treeArray);
            DA.SetDataTree(1, ptArray);
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
            get { return new Guid("532f1fab-c27e-4795-99d4-efb55cabc1d4"); }
        }
    }
}
