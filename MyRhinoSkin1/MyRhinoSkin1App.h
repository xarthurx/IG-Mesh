// MyRhinoSkin1.h : main header file for the MyRhinoSkin1 DLL.
//

#pragma once


#include "resource.h"		// main symbols

// CMyRhinoSkin1App
// See MyRhinoSkin1App.cpp for the implementation of this class
//

class CMyRhinoSkin1App : public CWinApp
{
public:
	CMyRhinoSkin1App() = default;

// Overrides
public:
	BOOL InitInstance() override;
	int ExitInstance() override;
	DECLARE_MESSAGE_MAP()
};

// CSplashWnd
// See MyRhinoSkin1App.cpp for the implementation of this class
//

class CSplashWnd : public CWnd
{
	DECLARE_DYNAMIC(CSplashWnd)

public:
	CSplashWnd();
	virtual ~CSplashWnd();

protected:
  CBitmap m_splash_bitmap;

protected:
	DECLARE_MESSAGE_MAP()

public:
  afx_msg void OnPaint();
  afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
};
