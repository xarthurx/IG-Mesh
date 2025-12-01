using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace igm {
public class IGM_planeDisplay : GH_Component {
  public IGM_planeDisplay()
      : base("Plane Coordinate Display",
             "igPlaneDisp",
             "Display a plane by its 3 coordinate axes with colors (X=Pink, Y=Green, Z=Blue).",
             "igMesh",
             "09::Utils") {}

  public override GH_Exposure Exposure => GH_Exposure.primary;
  protected override System.Drawing.Bitmap Icon => Properties.Resources.planeDisplay;
  public override Guid ComponentGuid => new Guid("6a3ca764-4429-4240-82b5-9dd1634b7b1c");

  protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
    pManager.AddPlaneParameter("Plane", "P", "Plane to display.", GH_ParamAccess.item);
    pManager.AddNumberParameter("Scale", "S", "Scale of the axes.", GH_ParamAccess.item, 1.0);
  }

  protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
    // This is a display-only component, no outputs
  }

  protected override void BeforeSolveInstance() {
    _lines.Clear();
    _widths.Clear();
    _colors.Clear();
  }

  protected override void SolveInstance(IGH_DataAccess DA) {
    Rhino.Geometry.Plane pln = Rhino.Geometry.Plane.Unset;
    double scale = double.NaN;

    if (!DA.GetData(0, ref pln)) {
      return;
    }
    if (!DA.GetData(1, ref scale)) {
      return;
    }

    if (!pln.IsValid || !Rhino.RhinoMath.IsValidDouble(scale)) {
      return;
    }

    Line lnX = new Line(pln.Origin, pln.XAxis * scale);
    Line lnY = new Line(pln.Origin, pln.YAxis * scale);
    Line lnZ = new Line(pln.Origin, pln.ZAxis * scale);

    _lines.Add(lnX);
    _lines.Add(lnY);
    _lines.Add(lnZ);

    _widths.Add(Math.Min(10, (int)(lnX.Length * 0.1) + 1));
    _widths.Add(Math.Min(10, (int)(lnY.Length * 0.1) + 1));
    _widths.Add(Math.Min(10, (int)(lnZ.Length * 0.1) + 1));

    _colors.Add(Color.DeepPink);
    _colors.Add(Color.LightGreen);
    _colors.Add(Color.DeepSkyBlue);
  }

  // Private members for display
  private readonly List<Line> _lines = new List<Line>();
  private readonly List<int> _widths = new List<int>();
  private readonly List<Color> _colors = new List<Color>();

  public override void DrawViewportWires(IGH_PreviewArgs args) {
    base.DrawViewportWires(args);

    for (int i = 0; i < _lines.Count; i++) {
      args.Display.DrawLine(_lines[i], _colors[i], _widths[i]);
      args.Display.DrawArrowHead(_lines[i].To, _lines[i].Direction, _colors[i], 0, _lines[i].Length * 0.2);
    }
  }
}
}
