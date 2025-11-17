using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace igm {
public class IGM_mapColorFace : GH_Component {
  /// <summary>
  /// Initializes a new instance of the MyComponent1 class.
  /// </summary>
  public IGM_mapColorFace()
      : base("Mesh Colours ~ Face",
             "igMeshColourFace",
             "Map a list of face color to mesh faces.",
             "igMesh",
             "04::Mapping") {}

  /// <summary>
  /// icon position in a category
  /// </summary>
  public override GH_Exposure Exposure => GH_Exposure.secondary;

  /// <summary>
  /// Registers all the input parameters for this component.
  /// </summary>
  protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
    pManager.AddMeshParameter("Mesh", "M", "Base mesh.", GH_ParamAccess.item);
    pManager.AddColourParameter("Color", "FC", "Face color.", GH_ParamAccess.list);
  }

  /// <summary>
  /// Registers all the output parameters for this component.
  /// </summary>
  protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {}

  /// <summary>
  /// This is the method that actually does the work.
  /// </summary>
  /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
  protected override void SolveInstance(IGH_DataAccess DA) {
    var mesh = new Rhino.Geometry.Mesh();
    if (!DA.GetData(0, ref mesh)) {
      return;
    }
    if (!mesh.IsValid) {
      AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Mesh.");
      return;
    }

    var FC = new List<System.Drawing.Color>();
    if (!DA.GetDataList<System.Drawing.Color>(1, FC)) {
      return;
    }

    if (FC.Count != mesh.Faces.Count) {
      AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        "Face number and color number does not match.");
    }

    renderMesh = new Rhino.Geometry.Mesh();

    // manually unweld the mesh. the default `unweld` for Rhino Mesh does not work properly.
    foreach (var (f, i) in mesh.Faces.Select((f, i) => (f, i))) {
      renderMesh.Vertices.Add(mesh.Vertices[f.A]);
      renderMesh.Vertices.Add(mesh.Vertices[f.B]);
      renderMesh.Vertices.Add(mesh.Vertices[f.C]);
      renderMesh.Vertices.Add(mesh.Vertices[f.D]);

      // same color for the four corners of the meshFace
      renderMesh.VertexColors.Add(FC[i]);
      renderMesh.VertexColors.Add(FC[i]);
      renderMesh.VertexColors.Add(FC[i]);
      renderMesh.VertexColors.Add(FC[i]);

      renderMesh.Faces.AddFace(i * 4, i * 4 + 1, i * 4 + 2, i * 4 + 3);
    }

    renderMesh.Normals.ComputeNormals();
    renderMesh.Compact();
  }

  public override void DrawViewportMeshes(IGH_PreviewArgs args) {
    args.Display.DrawMeshShaded(renderMesh,
                                new Rhino.Display.DisplayMaterial(System.Drawing.Color.Black));
    base.DrawViewportMeshes(args);
  }

  protected Rhino.Geometry.Mesh renderMesh;

  /// <summary>
  /// Provides an Icon for the component.
  /// </summary>
  protected override System.Drawing.Bitmap Icon {
    get {
      // You can add image files to your project resources and access them like this:
      return Properties.Resources.meshColourFace;
    }
  }

  /// <summary>
  /// Gets the unique ID for this component. Do not change this ID after release.
  /// </summary>
  public override Guid ComponentGuid {
    get { return new Guid("ff648905-f144-4d2e-b443-d51bc7b83258"); }
  }
}
}
