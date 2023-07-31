using Grasshopper.Kernel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace igmGH
{
    public class IGM_deformBiharmonic : GH_Component
    {
        /// <summary>
        /// Biharmoic deformation
        /// https://libigl.github.io/tutorial/#biharmonic-deformation 
        /// </summary>
        public IGM_deformBiharmonic()
          : base("Biharmonic Deformation.", "igBiharmonic",
              ".",
              "IG-Mesh", "05::deform")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("f2145d91-00bf-4935-8988-ad11f776c065");
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
            if (scalarF.Count != mesh.Vertices.Count)
            { return; }

            // call the cpp function to solve the adjacency list
            var sf = IGMRhinoCommon.Utils.RemapVtoF(ref mesh, scalarF);

            // output
            DA.SetDataList(0, sf);
        }
    }
}
