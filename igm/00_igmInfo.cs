using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace igm {
public class igmInfo : GH_AssemblyInfo {
  public override string Name => "igMesh";

  // Return a 24x24 pixel bitmap to represent this GHA library.
  public override Bitmap Icon => Properties.Resources.pluginIcon;

  // Return a short string describing the purpose of this GHA library.
  public override string Description =>
      "IG-Mesh is a one-stop solution for low-level (vertex-based, edge-based) mesh processing, " +
      "featuring many advanced algorithms from computer graphics community.";

  public override Guid Id => new Guid("9adf88b1-427d-4ce4-b72b-288d09821271");

  // Return a string identifying you or your company.
  public override string AuthorName => "Dr. Zhao MA";

  // Return a string representing your preferred contact details.
  public override string AuthorContact => "zhma@ethz.ch";

  // Return a string representing the version.  This returns the same version as the assembly.

  // public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
  public override string AssemblyVersion => "0.5.6";
  public override string Version => AssemblyVersion;
  public override GH_LibraryLicense License => GH_LibraryLicense.opensource;
}

public class IGM_CategoryIcon : GH_AssemblyPriority {
  public override GH_LoadingInstruction PriorityLoad() {
    try {
      var icon = Properties.Resources.pluginIcon;
      if (icon != null) {
        Grasshopper.Instances.ComponentServer.AddCategoryIcon("IG-Mesh", icon);
      }
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("IG-Mesh", 'I');
    } catch (Exception ex) {
      // Log error but don't prevent plugin from loading
      System.Diagnostics.Debug.WriteLine($"Failed to set category icon: {ex.Message}");
    }
    return GH_LoadingInstruction.Proceed;
  }
}
}
