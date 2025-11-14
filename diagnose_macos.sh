#!/bin/bash
# Diagnostic script for IG-Mesh plugin on macOS
# Run this script from the bin/Release/net8.0 directory or provide path as argument

echo "====================================="
echo "IG-Mesh macOS Diagnostic Tool"
echo "====================================="
echo ""

# Determine the directory to check
if [ -n "$1" ]; then
    PLUGIN_DIR="$1"
else
    PLUGIN_DIR="."
fi

echo "Checking directory: $PLUGIN_DIR"
echo ""

# Check for .gha file
echo "1. Checking for .gha plugin file..."
GHA_FILE=$(find "$PLUGIN_DIR" -name "*.gha" | head -n 1)
if [ -n "$GHA_FILE" ]; then
    echo "   ? Found: $GHA_FILE"
    echo "   Size: $(ls -lh "$GHA_FILE" | awk '{print $5}')"
else
    echo "   ? No .gha file found"
fi
echo ""

# Check for dylib
echo "2. Checking for native library (dylib)..."
DYLIB_FILE="$PLUGIN_DIR/libGeoSharPlusCPP.dylib"
if [ -f "$DYLIB_FILE" ]; then
    echo "   ? Found: $DYLIB_FILE"
    echo "   Size: $(ls -lh "$DYLIB_FILE" | awk '{print $5}')"
    
    # Check architecture
    echo ""
    echo "3. Checking dylib architecture..."
    lipo -info "$DYLIB_FILE"
    
    ARCH=$(uname -m)
    echo "   System architecture: $ARCH"
    if [ "$ARCH" = "arm64" ]; then
        if lipo -info "$DYLIB_FILE" | grep -q "arm64"; then
            echo "   ? Architecture matches system (Apple Silicon)"
        else
            echo "   ? Architecture mismatch! System is arm64 but dylib is not"
        fi
    elif [ "$ARCH" = "x86_64" ]; then
        if lipo -info "$DYLIB_FILE" | grep -q "x86_64"; then
            echo "   ? Architecture matches system (Intel)"
        else
            echo "   ? Architecture mismatch! System is x86_64 but dylib is not"
        fi
    fi
    
    echo ""
    echo "4. Checking dylib dependencies..."
    echo "   Dependencies:"
    otool -L "$DYLIB_FILE" | sed '1d' | while read -r line; do
        dep=$(echo "$line" | awk '{print $1}')
        echo "     - $dep"
        
        # Check if dependency exists (for non-system libraries)
        if [[ "$dep" != /usr/lib/* ]] && [[ "$dep" != /System/* ]] && [[ "$dep" != @rpath/* ]]; then
            if [ ! -f "$dep" ]; then
                echo "       ? NOT FOUND - may cause loading issues"
            fi
        fi
    done
    
    echo ""
    echo "5. Checking for Gatekeeper quarantine..."
    if xattr -l "$DYLIB_FILE" | grep -q "com.apple.quarantine"; then
        echo "   ? Library is quarantined by macOS Gatekeeper"
        echo "   To remove quarantine, run:"
        echo "   xattr -d com.apple.quarantine '$DYLIB_FILE'"
    else
        echo "   ? No quarantine flag"
    fi
    
    echo ""
    echo "6. Checking code signature..."
    if codesign -v "$DYLIB_FILE" 2>&1 | grep -q "valid on disk"; then
        echo "   ? Library is properly code signed"
        codesign -d -vv "$DYLIB_FILE" 2>&1 | grep "Authority"
    else
        echo "   ? Library is not code signed"
        echo "   This may cause security warnings on macOS"
    fi
    
else
    echo "   ? libGeoSharPlusCPP.dylib not found!"
    echo "   Expected location: $DYLIB_FILE"
    echo ""
    echo "   Searching for dylib in parent directories..."
    find "$(dirname "$PLUGIN_DIR")" -name "libGeoSharPlusCPP.dylib" 2>/dev/null
fi

echo ""
echo "7. Checking assembly dependencies..."
if [ -n "$GHA_FILE" ]; then
    DEPS=$(find "$PLUGIN_DIR" -name "*.dll" -not -name "GeoSharPlusCPP.dll")
    if [ -n "$DEPS" ]; then
        echo "   Found .NET dependencies:"
        echo "$DEPS" | while read -r dep; do
            echo "     - $(basename "$dep")"
        done
    fi
fi

echo ""
echo "8. Checking Grasshopper libraries directory..."
GH_LIB_DIR="$HOME/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries"
if [ -d "$GH_LIB_DIR" ]; then
    echo "   ? Grasshopper libraries directory exists"
    if [ -d "$GH_LIB_DIR/igmGH" ] || [ -d "$GH_LIB_DIR/IG-Mesh" ]; then
        echo "   ? Plugin installation detected"
    else
        echo "   ? Plugin not found in Grasshopper libraries"
    fi
else
    echo "   ? Grasshopper libraries directory not found"
fi

echo ""
echo "====================================="
echo "Diagnostic Complete"
echo "====================================="
echo ""
echo "Common Issues and Solutions:"
echo ""
echo "• Architecture mismatch:"
echo "  Rebuild the C++ library for the correct architecture"
echo ""
echo "• Quarantined library:"
echo "  Run: xattr -d com.apple.quarantine <path-to-dylib>"
echo ""
echo "• Missing dependencies:"
echo "  Ensure all C++ dependencies are statically linked or bundled"
echo ""
echo "• Not code signed:"
echo "  For distribution, code sign the library with Developer ID"
echo ""
echo "To test library loading manually:"
echo "  Python: import ctypes; ctypes.CDLL('$DYLIB_FILE')"
echo ""
