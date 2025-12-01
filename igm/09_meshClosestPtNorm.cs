using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace igm {
public class IGM_meshClosestPtNorm : GH_Component {
  public IGM_meshClosestPtNorm()
      : base("Mesh Closest Point Normal",
             "igClosestPtNorm",
             "Get the normal vector on the mesh from the closest point of a given point.",
             "igMesh",
             "09::Utils") {}

  public override GH_Exposure Exposure => GH_Exposure.primary;
  protected override System.Drawing.Bitmap Icon => Properties.Resources.meshClosestPtNorm;
  public override Guid ComponentGuid => new Guid("f6f25007-3035-423c-b21f-bb4976333f91");

  protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
    pManager.AddMeshParameter("Mesh", "M", "Base mesh for calculation.", GH_ParamAccess.item);
    pManager.AddPointParameter("Point", "P", "Point to calculate from.", GH_ParamAccess.item);
  }

  protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
    pManager.AddPointParameter("Closest Point", "CP", "Closest point on the mesh.", GH_ParamAccess.item);
    pManager.AddVectorParameter("Normal", "N", "Normal vector at the closest point.", GH_ParamAccess.item);
  }

  protected override void SolveInstance(IGH_DataAccess DA) {
    Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
    Rhino.Geometry.Point3d pt = Rhino.Geometry.Point3d.Unset;

    if (!DA.GetData(0, ref mesh)) {
      return;
    }
    if (!DA.GetData(1, ref pt)) {
      return;
    }

    if (!mesh.IsValid || !pt.IsValid) {
      return;
    }

    MeshPoint mPt = mesh.ClosestMeshPoint(pt, 0.0);
    Vector3d nrml = mesh.NormalAt(mPt);

    DA.SetData(0, mPt.Point);
    DA.SetData(1, nrml);
  }
}
}
