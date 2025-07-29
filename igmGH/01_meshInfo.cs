using Grasshopper.Kernel;
using System;

namespace igmGH {
  public class IGM_mesh_properties : GH_Component {
    /// <summary>
    /// Initializes a new instance of the MyComponent1 class.
    /// </summary>
    public IGM_mesh_properties()
        : base("Mesh Info", "igMeshInfo", "Provide various mesh info: V, F, centroid, volume.",
               "IG-Mesh", "01::Info+IO") {}
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.meshInfo;
    public override Guid ComponentGuid => new Guid("80bb92aa-6cc4-4cd0-bfa8-a841ecbb9da8");

    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
      pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
      pManager.AddPointParameter("Vertex", "V", "The vertices of the input mesh.",
                                 GH_ParamAccess.list);
      pManager.AddIntegerParameter(
          "Face", "F", "The face of the input mesh as three indices into V.", GH_ParamAccess.tree);
      pManager.AddPointParameter("Centroid", "cen",
                                 "The centroid using surface integral if the input mesh is closed.",
                                 GH_ParamAccess.item);
      pManager.AddNumberParameter("Volume", "vol",
                                  "The volume of the mesh if the input mesh is closed.",
                                  GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
      if (!DA.GetData(0, ref mesh)) {
        return;
      }
      if (!mesh.IsValid) {
        return;
      }

      // call the cpp function to solve the adjacency list
      var (V, F, cen, vol) = GeoSharpNET.MeshUtils.GetMeshInfo(ref mesh);

      Grasshopper.DataTree<int> fTree = new Grasshopper.DataTree<int>();
      for (int i = 0; i < F.Count; i++) {
        var path = new Grasshopper.Kernel.Data.GH_Path(i);
        fTree.AddRange(F[i], path);
      }
      // output
      DA.SetDataList(0, V);
      DA.SetDataTree(1, fTree);
      DA.SetData(2, cen);
      DA.SetData(3, vol);
    }
  }
}
