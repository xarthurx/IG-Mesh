using Grasshopper.Kernel;
using System;

namespace igmGH {
public class IGM_random_points_on_mesh : GH_Component {
  /// <summary>
  /// Initializes a new instance of the MyComponent1 class.
  /// </summary>
  public IGM_random_points_on_mesh()
      : base("Random Pt On Mesh",
             "igRndPt",
             "Randomly sample N points on surface of the given mesh with random/uniform " +
                 "distribution.",
             "IG-Mesh",
             "09::Utils") {}

  /// <summary>
  /// icon position in a category
  /// </summary>
  public override GH_Exposure Exposure => GH_Exposure.secondary;

  /// <summary>
  /// Registers all the input parameters for this component.
  /// </summary>
  protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
    pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
    pManager.AddIntegerParameter(
        "Number", "N", "Number of sampled points.", GH_ParamAccess.item, 0);
    pManager[1].Optional = true;

    // the uniform method uses a blue-noise (Poisson's disk) approach to sample the points.
    // pManager.AddIntegerParameter("Method", "M", "The method used for sampling: 0-random;
    // 1-uniform.", GH_ParamAccess.item, 0); pManager[2].Optional = true;
  }

  /// <summary>
  /// Registers all the output parameters for this component.
  /// </summary>
  protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
    pManager.AddPointParameter("Sampled Points", "P", "The N sampled points.", GH_ParamAccess.list);
    pManager.AddIntegerParameter("Face Index",
                                 "FI",
                                 "The corresponding face indices of the sampled points.",
                                 GH_ParamAccess.list);
  }

  /// <summary>
  /// This is the method that actually does the work.
  /// </summary>
  /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
  protected override void SolveInstance(IGH_DataAccess DA) {
    Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
    if (!DA.GetData(0, ref mesh)) {
      return;
    }
    if (!mesh.IsValid) {
      return;
    }

    int N = 1;
    if (!DA.GetData(1, ref N) || N == 0) {
      return;
    }

    int M = (mode == "random" ? 0 : 1);
    // if (!DA.GetData(2, ref M))
    //{ return; }

    // call the GeoSharPlusNET function to sample points on mesh
    var (p, fi) = GeoSharpNET.MeshUtils.GetRandomPointsOnMesh(ref mesh, N, M);

    // output
    DA.SetDataList(0, p);
    DA.SetDataList(1, fi);
  }

  protected override void BeforeSolveInstance() {
    Message = mode.ToUpper();
  }

  public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu) {
    base.AppendAdditionalMenuItems(menu);

    Menu_AppendSeparator(menu);
    Menu_AppendItem(menu, "Mode:", (sender, e) => {}, false).Font = GH_FontServer.StandardItalic;
    Menu_AppendItem(menu,
                    " Random",
                    (sender, e) => Helper.SelectMode(this, sender, e, ref mode, "random"),
                    true,
                    CheckMode("random"));
    Menu_AppendItem(menu,
                    " Uniform",
                    (sender, e) => Helper.SelectMode(this, sender, e, ref mode, "uniform"),
                    true,
                    CheckMode("uniform"));
  }

  public override bool Write(GH_IO.Serialization.GH_IWriter writer) {
    if (mode != "")
      writer.SetString("mode", mode);

    return base.Write(writer);
  }
  public override bool Read(GH_IO.Serialization.GH_IReader reader) {
    if (reader.ItemExists("mode"))
      mode = reader.GetString("mode");

    Message = reader.GetString("mode").ToUpper();

    return base.Read(reader);
  }

  private bool CheckMode(string _modeCheck) => mode == _modeCheck;

  private string mode = "random";  // "random" or "uniform"

  protected override System.Drawing.Bitmap Icon => Properties.Resources.meshRandomPtsOnMesh;
  public override Guid ComponentGuid => new Guid("5819dc11-ccff-41eb-b126-96c34911ddc1");
}
}
