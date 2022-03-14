using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace igl_GrassHopper
{
    public class iglGH_meshIsoLine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public iglGH_meshIsoLine()
          : base("mesh isoline", "isoline",
              "extract the isolines of a given mesh",
              "IGL+", "utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Constraint Indices", "I", "the indices to be constrained", GH_ParamAccess.list);
            pManager.AddNumberParameter("Constraint Values", "V", "the values to constrain with", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of Isolines", "N", "the number of isolines", GH_ParamAccess.item, 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Iso Curves", "C", "the extracted isolines from input mesh", GH_ParamAccess.list);
            pManager.AddPointParameter("Isoline Points", "P", "extracted points on isolines", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            List<int> con_idx = new List<int>();
            List<float> con_val = new List<float>();
            int divN = 1;

            if (!DA.GetData(0, ref mesh)) { return; }
            if (!mesh.IsValid) { return; }

            if (!DA.GetDataList(1, con_idx)) { return; }
            if (!DA.GetDataList(2, con_val)) { return; }
            if (!(con_idx.Count > 0) || !(con_val.Count > 0)) { return; }
            if (con_idx.Count != con_val.Count) { return; }
            // TODO: add warning message
            if (!DA.GetData(3, ref divN)) { return; }

            // call the cpp function to solve the adjacency list
            var res = IGLRhinoCommon.Utils.getIsolinePts(ref mesh, ref con_idx, ref con_val, divN);

            // construct the index & pt tree from the adjacency list
            Grasshopper.DataTree<Point3d> ptTree = new Grasshopper.DataTree<Point3d>();
            Grasshopper.DataTree<Curve> crvTree = new Grasshopper.DataTree<Curve>();
            for (int i = 0; i < res.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                ptTree.AddRange(res[i], path);

                // if has move than 2 pts, interpolate curves
                if (res[i].Count > 3)
                {
                    var crv = Curve.CreateInterpolatedCurve(res[i], 3);
                    crvTree.Add(crv, path);
                }
                else if (res[i].Count > 2)
                {
                    var crv = Curve.CreateInterpolatedCurve(res[i], 2);
                    crvTree.Add(crv, path);
                }
            }

            // assign to the output
            DA.SetDataTree(0, crvTree);
            DA.SetDataTree(1, ptTree);
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
                return Properties.Resources.meshIsoline;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e0086ab2-5d49-42b6-991c-92b2cd95faf3"); }
        }
    }
}