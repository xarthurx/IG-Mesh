using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.IO;

namespace igmGH {
public class igmInfo : GH_AssemblyInfo {
  public override string Name => "IG-Mesh";

  public override Bitmap Icon {
    get {
      try {
        return Properties.Resources.pluginIcon;
      } catch {
        // Icon loading failed - return null to avoid breaking assembly load
        return null;
      }
    }
  }

  public override Bitmap AssemblyIcon {
    get {
      try {
        return Properties.Resources.pluginIcon;
      } catch {
        return null;
      }
    }
  }

  public override string Description =>
      "IG-Mesh is a one-stop solution for low-level (vertex-based, edge-based) mesh processing, " +
      "featuring many advanced algorithms from computer graphics community.";
  public override string AuthorName => "Dr. Zhao MA";
  public override string AuthorContact => "https://github.com/xarthurx/IG-Mesh";

  public override string AssemblyVersion => "0.5.6";
  public override string Version => AssemblyVersion;
  public override Guid Id => new Guid("18bfce35-2f9a-4442-9028-9d0821505dcf");
  public override GH_LibraryLicense License => GH_LibraryLicense.opensource;
}

// update plugin icons in the tab
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

public class igmInfoExporter : GH_Component {
  public igmInfoExporter()
      : base("IG-Mesh Libarary Info",
             "igLibInfo",
             "Check IGM version and whether everything is loaded correctly on all platforms.",
             "IG-Mesh",
             "00::info") {}

  public override GH_Exposure Exposure => GH_Exposure.primary;
  // protected override System.Drawing.Bitmap Icon => null;
  public override Guid ComponentGuid => new Guid("97a612f9-3fb9-4eb4-a4ed-ec988ac8dab6");

  protected override void RegisterInputParams(GH_InputParamManager pManager) {}

  protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
    pManager.AddTextParameter(
        "Info", "info", "Plugin info for loading native lib.", GH_ParamAccess.item);
  }

  protected override void SolveInstance(IGH_DataAccess DA) {
    try {
      // Check if the native library is loaded before proceeding
      if (!GSP.NativeBridge.IsNativeLibraryLoaded) {
        string[] errors = GSP.NativeBridge.GetErrorMessages();
        string errorMsg = string.Join("\n", errors);
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                          $"Native library not loaded. Check the following errors:\n{errorMsg}");
        return;
      } else {
        // Create an instance of BeingAliveLanguageInfo to access the non-static property
        string version = new igmInfo().AssemblyVersion;
        string loadedPath = GSP.NativeBridge.LoadedLibraryPath;
        string libName = Path.GetFileName(loadedPath);

        string infoText = $"Native library loaded successfully from: {loadedPath}\n" +
                          $"Library name: {libName}\n" + $"Version: {version}";

        DA.SetData(0, infoText);
      }

    } catch (Exception ex) {
      AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Exception: {ex.Message}");
    }
  }
}
}
