using Grasshopper.Kernel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace igmGH
{
    public class IGM_paramHarmonic : GH_Component
    {
        /// <summary>
        /// Biharmoic deformation
        /// https://libigl.github.io/tutorial/#biharmonic-deformation 
        /// </summary>
        public IGM_paramHarmonic()
          : base("Harmonic Parametrization.", "igParamHarmonic",
              "Harmonic parametrization of a mesh into a circular shape.",
              "IG-Mesh", "07::parametrization")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("bc1ad1ab-be4f-4b3e-9beb-b15729db8b53");
        //protected override System.Drawing.Bitmap Icon => Properties.Resources.null;
        protected override System.Drawing.Bitmap Icon => null;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Base mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scalar", "S", "Scalar defined on vertices.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Remaped Scalar", "SF", "The remapped valuse on faces.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            if (!DA.GetData(0, ref mesh))
            { return; }
            if (!mesh.IsValid)
            { return; }

            List<double> scalarF = new List<double>();
            if (!DA.GetDataList<double>(1, scalarF))
            { return; }

            // call the cpp function to solve the adjacency list
            var v_uv = IGMRhinoCommon.Utils.GetParamHarmonic(ref mesh, k);

            // output
            DA.SetDataList(0, v_uv);
        }
    }
}
