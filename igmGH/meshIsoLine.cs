using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace igmGH
{
    public class meshIsoLine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public meshIsoLine()
          : base("Isoline", "iIsoline",
              "Extract the isolines of a given mesh.",
              "IG-Mesh", "05 | Query")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "input mesh to analysis.", GH_ParamAccess.item);

            pManager.AddIntegerParameter("Constraint Index", "conI", "Vertex indices to be constrained.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Constraint Value", "conV", "Values (in [0, 1]) to constrain with.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Isoline t", "t", "Interpretation parameters (in [0, 1]) of isolines.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Isoline Point", "P", "Extracted points on isolines.", GH_ParamAccess.tree);
            //pManager.AddCurveParameter("Isoline Curve", "C", "The extracted isolines from input mesh.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mesh Scalar", "S", "Scalar values for the vertices.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            List<int> con_idx = new List<int>();
            List<double> con_val = new List<double>();
            List<double> iso_t = new List<double>();

            if (!DA.GetData(0, ref mesh)) { return; }
            if (!mesh.IsValid) { return; }

            if (!DA.GetDataList(1, con_idx)) { return; }
            if (!DA.GetDataList(2, con_val)) { return; }

            if (con_idx.Count <= 0 || con_val.Count <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "# of vertices and constrained values need to be > 0.");
                return;

            }
            if (con_idx.Count != con_val.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "# of vertices and # of corresponding constrained values do not match.");
                return;
            }

            foreach (var val in con_val)
            {
                if (val < 0 || val > 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "constrained value should be in [0, 1].");
                    return;
                }
            }

            if (!DA.GetDataList(3, iso_t)) { return; }

            // call the cpp function to solve the adjacency list
            var (isoPts, scalarV) = IGMRhinoCommon.Utils.getIsolinePts(ref mesh, ref con_idx, ref con_val, ref iso_t);

            // construct the index & pt tree from the adjacency list
            Grasshopper.DataTree<Point3d> ptTree = new Grasshopper.DataTree<Point3d>();
            //Grasshopper.DataTree<Curve> crvTree = new Grasshopper.DataTree<Curve>();
            for (int i = 0; i < isoPts.Count; i++)
            {
                var path = new Grasshopper.Kernel.Data.GH_Path(i);
                ptTree.AddRange(isoPts[i], path);

                // if has move than 2 pts, interpolate curves
                //if (res[i].Count > 3)
                //{
                //    var crv = Curve.CreateInterpolatedCurve(res[i], 3);
                //    crvTree.Add(crv, path);
                //}
                //else if (res[i].Count > 2)
                //{
                //    var crv = Curve.CreateInterpolatedCurve(res[i], 2);
                //    crvTree.Add(crv, path);
                //}
            }

            // assign to the output
            //DA.SetDataTree(0, crvTree);
            DA.SetDataTree(0, ptTree);
            DA.SetDataList(1, scalarV);
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