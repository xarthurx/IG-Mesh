﻿using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace igmGH
{
    public class IGM_BoundLoop : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IGM_Barycenter class.
        /// </summary>
        public IGM_BoundLoop()
          : base("Boundary Loop Points", "igBoundLoop",
              "compute the boundary loop of the given mesh.",
              "IG-Mesh", "03 | Adjacency+Bound")
        {
        }

        /// <summary>
        /// icon position in a category
        /// </summary>
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
            pManager.AddIntegerParameter("Boundary Index", "B", "the boundary list of the input mesh", GH_ParamAccess.tree);
            pManager.AddPointParameter("Boundary Vertex", "P", "the boundary vertices of the input mesh", GH_ParamAccess.tree);
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
            var res = IGMRhinoCommon.Utils.getBoundaryLoop(ref mesh);

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
                return Properties.Resources.meshBoundLoop;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3A2ADC95-6FDB-41CC-8B5B-A611AE4B08E9"); }
        }
    }
}