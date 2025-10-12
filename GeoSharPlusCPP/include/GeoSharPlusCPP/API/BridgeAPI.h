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
GEOSHARPLUS_API bool GEOSHARPLUS_CALL point3d_array_roundtrip(const uint8_t* inBuffer,
                                                              int inSize,
                                                              uint8_t** outBuffer,
                                                              int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL mesh_roundtrip(const uint8_t* inBuffer,
                                                     int inSize,
                                                     uint8_t** outBuffer,
                                                     int* outSize);

// ! --------------------------------
// ! 01:: IO, property funcs
// ! --------------------------------

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_read_triangle_mesh(const char* filename,
                                                             uint8_t** outBuffer,
                                                             int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_write_triangle_mesh(const uint8_t* inBuffer,
                                                              const int inSize,
                                                              const char* filename);

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

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_corner_normals(
    const uint8_t* inBuffer, int inSize, double threshold_deg, uint8_t** outBuffer, int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_edge_normals(const uint8_t* inBuffer,
                                                       int inSize,
                                                       int weightingType,
                                                       uint8_t** obEN,
                                                       int* obsEN,
                                                       uint8_t** obEI,
                                                       int* obsEI,
                                                       uint8_t** obEMAP,
                                                       int* obsEMAP);

// ! --------------------------------
// ! 03:: adjacency funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_vert_vert_adjacency(const uint8_t* inBuffer,
                                                              int inSize,
                                                              uint8_t** outBuffer,
                                                              int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_vert_tri_adjacency(const uint8_t* inBuffer,
                                                             int inSize,
                                                             uint8_t** outBufferVT,
                                                             int* outSizeVT,
                                                             uint8_t** outBufferVTI,
                                                             int* outSizeVTI);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_tri_tri_adjacency(const uint8_t* inBuffer,
                                                            int inSize,
                                                            uint8_t** outBufferTT,
                                                            int* outSizeTT,
                                                            uint8_t** outBufferTTI,
                                                            int* outSizeTTI);

// ! --------------------------------
// ! 03:: boundary funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_boundary_loop(const uint8_t* inBuffer,
                                                        int inSize,
                                                        uint8_t** outBuffer,
                                                        int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_boundary_facet(const uint8_t* inBuffer,
                                                         int inSize,
                                                         uint8_t** outBufferEL,
                                                         int* outSizeEL,
                                                         uint8_t** outBufferTL,
                                                         int* outSizeTL);
// ! --------------------------------
// ! 04:: scalar remap funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_remap_VtoF(const uint8_t* inBufferMesh,
                                                     int inSizeMesh,
                                                     const uint8_t* inBufferScalar,
                                                     int inSizeScalar,
                                                     uint8_t** outBuffer,
                                                     int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_remap_FtoV(const uint8_t* inBufferMesh,
                                                     int inSizeMesh,
                                                     const uint8_t* inBufferScalar,
                                                     int inSizeScalar,
                                                     uint8_t** outBuffer,
                                                     int* outSize);

// ! --------------------------------
// ! 05:: curvature funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_principal_curvature(const uint8_t* inBuffer,
                                                              int inSize,
                                                              uint32_t radius,
                                                              uint8_t** outBufferPD1,
                                                              int* outSizePD1,
                                                              uint8_t** outBufferPD2,
                                                              int* outSizePD2,
                                                              uint8_t** outBufferPV1,
                                                              int* outSizePV1,
                                                              uint8_t** outBufferPV2,
                                                              int* outSizePV2);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_gaussian_curvature(const uint8_t* inBuffer,
                                                             int inSize,
                                                             uint8_t** outBuffer,
                                                             int* outSize);

// ! --------------------------------
// ! 06:: measure funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_fast_winding_number(const uint8_t* inBufferMesh,
                                                              int inSizeMesh,
                                                              const uint8_t* inBufferPoints,
                                                              int inSizePoints,
                                                              uint8_t** outBuffer,
                                                              int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_signed_distance(const uint8_t* inBufferMesh,
                                                          int inSizeMesh,
                                                          const uint8_t* inBufferPoints,
                                                          int inSizePoints,
                                                          int signedType,
                                                          uint8_t** outBufferSD,
                                                          int* outSizeSD,
                                                          uint8_t** outBufferFI,
                                                          int* outSizeFI,
                                                          uint8_t** outBufferCP,
                                                          int* outSizeCP);

// ! --------------------------------
// ! 07:: quad mesh funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_quad_planarity(const uint8_t* inBuffer,
                                                         int inSize,
                                                         uint8_t** outBuffer,
                                                         int* outSize);

GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_planarize_quad_mesh(const uint8_t* inBuffer,
                                                              int inSize,
                                                              int maxIter,
                                                              double threshold,
                                                              uint8_t** outBuffer,
                                                              int* outSize);

// ! --------------------------------
// ! 08:: laplacian funcs
// ! --------------------------------
GEOSHARPLUS_API bool GEOSHARPLUS_CALL IGM_laplacian_scalar(const uint8_t* inBufferMesh,
                                                           int inSizeMesh,
                                                           const uint8_t* inBufferIndices,
                                                           int inSizeIndices,
                                                           const uint8_t* inBufferValues,
                                                           int inSizeValues,
                                                           uint8_t** outBuffer,
                                                           int* outSize);

}  // extern "C"
