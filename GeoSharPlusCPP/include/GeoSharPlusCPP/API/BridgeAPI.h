#pragma once
#include <cstdint>

#include "GeoSharPlusCPP/Core/Macro.h"

extern "C" {

// ! --------------------------------
// ! Testing Function for GSP.
// ! --------------------------------
// Conduct a roundtrip serialization of a point3d
GEOSHARPLUS_API bool GEOSHARPLUS_CALL point3d_roundtrip(const uint8_t* InBuffer,
                                                        int inSize,
                                                        uint8_t** outBuffer,
                                                        int* outSize);
// Conduct a roundtrip serialization of a point array
GEOSHARPLUS_API bool GEOSHARPLUS_CALL point3d_array_roundtrip(
    const uint8_t* inBuffer, int inSize, uint8_t** outBuffer, int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL mesh_roundtrip(const uint8_t* inBuffer,
                                                     int inSize,
                                                     uint8_t** outBuffer,
                                                     int* outSize);

// ! --------------------------------
// ! 01:: IO, property funcs
// ! --------------------------------

GEOSHARPLUS_API bool GEOSHARPLUS_CALL
IGM_read_triangle_mesh(const char* filename, uint8_t** outBuffer, int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_write_triangle_mesh(
    const uint8_t* inBuffer, const int inSize, const char* filename);

// lculate the centroid of a mesh (igl function)
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_centroid(const uint8_t* inBuffer,
                                                   int inSize,
                                                   uint8_t** outBuffer,
                                                   int* outSize);
// ! --------------------------------
// ! 02:: centre, normal funcs
// ! --------------------------------

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_barycenter(const uint8_t* inBuffer,
                                                     int inSize,
                                                     uint8_t** outBuffer,
                                                     int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_vert_normals(const uint8_t* inBuffer,
                                                       int inSize,
                                                       uint8_t** outBuffer,
                                                       int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_face_normals(const uint8_t* inBuffer,
                                                       int inSize,
                                                       uint8_t** outBuffer,
                                                       int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL
IGM_corner_normals(const uint8_t* inBuffer, int inSize, double threshold_deg,
                   uint8_t** outBuffer, int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_edge_normals(
    const uint8_t* inBuffer, int inSize, int weightingType, uint8_t** obEN,
    int* obsEN, uint8_t** obEI, int* obsEI, uint8_t** obEMAP, int* obsEMAP);
}
