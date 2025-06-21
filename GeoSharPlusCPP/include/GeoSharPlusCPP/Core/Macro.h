#pragma once

#ifdef _WIN32
    #ifdef GEOSHARPLUS_EXPORTS
        #define GEOSHARPLUS_API __declspec(dllexport)
    #else
        #define GEOSHARPLUS_API __declspec(dllimport)
    #endif
    #define GEOSHARPLUS_CALL __stdcall
#else
    #define GEOSHARPLUS_API __attribute__((visibility("default")))
    #define GEOSHARPLUS_CALL
#endif
