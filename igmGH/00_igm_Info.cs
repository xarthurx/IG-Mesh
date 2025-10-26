using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace igmGH {
public class igmInfo : GH_AssemblyInfo {
  public override string Name => "IG-Mesh";
  public override Bitmap Icon => Properties.Resources.pluginIcon;
  public override Bitmap AssemblyIcon => Properties.Resources.pluginIcon;
  public override string Description =>
      "IG-Mesh is a one-stop solution for low-level (vertex-based, edge-based) mesh processing, " +
      "featuring many advanced algorithms from computer graphics community.";
  public override string AuthorName => "Dr. Zhao MA";
  public override string AuthorContact => "https://github.com/xarthurx/IG-Mesh";
  // controls the package manager version.
  // public override string Version =>
  //    System.Diagnostics.FileVersionInfo
  //        .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
  //        .FileVersion;

  public override string Version => "0.5.0";
  public override Guid Id => new Guid("18bfce35-2f9a-4442-9028-9d0821505dcf");
  public override GH_LibraryLicense License => GH_LibraryLicense.opensource;
}

// update plugin icons in the tab
public class IGM_CategoryIcon : GH_AssemblyPriority {
  public override GH_LoadingInstruction PriorityLoad() {
    Grasshopper.Instances.ComponentServer.AddCategoryIcon("IG-Mesh",
                                                          Properties.Resources.pluginIcon);
    Grasshopper.Instances.ComponentServer.AddCategorySymbolName("IG-Mesh", 'I');
    return GH_LoadingInstruction.Proceed;
  }
}
}
