// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently

#pragma once


// This Skin DLL is Rhino 6 ready
#define RHINO_V6_READY

// Skin DLLs must use the release version of MFC used by Rhino.
// Skin DLLs that require debugging information need to be built with
// RHINO_DEBUG_PLUGIN defined.

// Rhino SDK Preamble
#include "RhinoSdkStdafxPreamble.h"

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS       // some CString constructors will be explicit

#include <afxwin.h>                              // MFC core and standard components
#include <afxext.h>                              // MFC extensions




#include <afxdtctl.h>                            // MFC support for Internet Explorer 4 Common Controls

// TODO: include additional commonly used header files here


// Rhino SDK classes
#include "RhinoSdk.h" 

// Rhino Render Development Kit (RDK) classes
#include "RhRdkHeaders.h" 

// TODO: include additional Rhino-related header files here


// Rhino SDK linking pragmas
#include "rhinoSdkPlugInLinkingPragmas.h"
