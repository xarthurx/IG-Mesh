# Copilot Instructions for igMesh Project

## Project Overview

**igMesh** is a Grasshopper plugin for Rhino that provides advanced mesh processing capabilities. It implements many algorithms from the computer graphics community for vertex-based and edge-based mesh operations.

## Project Structure

```
igMesh/
├── igm/                      # Main Grasshopper plugin project
│   ├── 00_igmInfo.cs         # Plugin info and category registration
│   ├── 01_*.cs               # I/O and basic mesh info components
│   ├── 02_*.cs               # Mesh properties (normals, barycenter)
│   ├── 03_*.cs               # Mesh topology (adjacency, bounds)
│   ├── 04_*.cs               # Mesh coloring and scalar operations
│   ├── 05_*.cs               # Utility components
│   ├── 06_*.cs               # Advanced algorithms (curvature, SDF, etc.)
│   ├── 07_*.cs               # Parametrization
│   ├── 09_*.cs               # Mesh utilities
│   ├── Utils.cs              # Helper utilities
│   └── Properties/           # Resources and icons
│       ├── icons/            # 24x24 PNG icons for components
│       ├── Resources.resx    # Resource definitions
│       └── Resources.Designer.cs
│
├── GeoSharPlusCPP/           # C++ native library (supporting project)
│   ├── CMakeLists.txt        # CMake build configuration
│   ├── vcpkg.json            # C++ dependencies via vcpkg
│   ├── include/              # Header files
│   ├── src/                  # C++ source files
│   │   ├── API/              # Public API functions
│   │   ├── Core/             # Core mesh algorithms
│   │   └── Serialization/    # FlatBuffers serialization
│   └── schema/               # FlatBuffers schema definitions (.fbs)
│
├── GeoSharPlusNET/           # .NET bridge library (supporting project)
│   ├── NativeBridge.cs       # P/Invoke declarations
│   ├── Wrapper.cs            # High-level wrappers
│   └── MeshUtils.cs          # Mesh utility functions
│
├── scripts/                  # Build scripts
│   ├── prebuild.ps1          # Pre-build script (copies native DLLs)
│   ├── postbuild.ps1         # Post-build script
│   └── prepare_yak_pkg.ps1   # Yak package preparation
│
└── bin/                      # Build output
```

## Architecture

### Data Flow Between C# and C++

1. **GeoSharPlusCPP**: Native C++ library implementing mesh algorithms

   - Uses FlatBuffers for efficient data serialization
   - Exposes C-style API functions via `extern "C"`
   - Built with CMake and vcpkg for dependency management

2. **GeoSharPlusNET**: .NET bridge library

   - Uses P/Invoke (`DllImport`) to call native functions
   - Handles marshalling of data between managed and unmanaged code
   - FlatBuffers schemas are shared between C++ and C# sides

3. **igm (igMesh)**: Main Grasshopper plugin
   - Implements `GH_Component` classes for Grasshopper
   - Uses `GeoSharPlusNET` for native algorithm calls
   - Handles Rhino geometry types (`Mesh`, `Point3d`, `Vector3d`)

### Key Libraries/Dependencies

- **C++ Side**: libigl, Eigen, FlatBuffers, vcpkg packages
- **C# Side**: Grasshopper SDK, RhinoCommon, FlatBuffers

## Coding Conventions

### Component File Naming

- Files are prefixed with numbers indicating category: `XX_componentName.cs`
- Categories:
  - `00_` - Plugin info
  - `01_` - I/O operations
  - `02_` - Basic properties
  - `03_` - Topology
  - `04_` - Coloring/scalar
  - `05_` - Utilities
  - `06_` - Advanced algorithms
  - `07_` - Parametrization
  - `09_` - Mesh utilities

### GH_Component Pattern

```csharp
namespace igm {
public class IGM_ComponentName : GH_Component {
  public IGM_ComponentName()
      : base("Display Name",
             "igNickname",
             "Component description.",
             "igMesh",
             "XX::Category") {}

  public override GH_Exposure Exposure => GH_Exposure.primary;
  protected override System.Drawing.Bitmap Icon => Properties.Resources.iconName;
  public override Guid ComponentGuid => new Guid("unique-guid-here");

  protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
    // Add inputs
  }

  protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
    // Add outputs
  }

  protected override void SolveInstance(IGH_DataAccess DA) {
    // Implementation
  }
}
}
```

### Code Style

- Use C# 10+ features where appropriate
- Opening braces on same line for namespaces and classes
- Use expression-bodied members for simple properties
- Validate inputs early with `return` on failure

## Adding New Components

1. Create new `.cs` file with appropriate prefix (e.g., `05_utilNewFeature.cs`)
2. Add 24x24 PNG icon to `igm/Properties/icons/`
3. Add icon reference to `Resources.resx`
4. Add property accessor in `Resources.Designer.cs`
5. Implement `GH_Component` with unique GUID

## Adding Native Algorithms

1. Implement algorithm in `GeoSharPlusCPP/src/Core/`
2. Create API wrapper in `GeoSharPlusCPP/src/API/`
3. Add FlatBuffers schema if new data types needed
4. Add P/Invoke declaration in `GeoSharPlusNET/NativeBridge.cs`
5. Create managed wrapper in `GeoSharPlusNET/`
6. Use wrapper in Grasshopper component

## Building

### Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK
- CMake 3.20+
- vcpkg (for C++ dependencies)

### Build Order

1. Build `GeoSharPlusCPP` (CMake)
2. Build solution (the prebuild script copies native DLLs)

## Testing

- Test Grasshopper definitions are in `scripts/IGM_examples.gh`
- Load the plugin via `_GrasshopperDeveloperSettings` in Rhino

## Notes

- The plugin targets .NET 8.0 for Rhino 8 compatibility
- Icons must be 24x24 pixels
- Component GUIDs must never change after release
- The native DLL must be in the same directory as the .gha file
