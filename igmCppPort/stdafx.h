// -------------------------------------
// Open Source OpenNURBS
// -------------------------------------
#define NOMINMAX  // avoid the min/max macro from windows.h

// defining OPENNURBS_PUBLIC_INSTALL_DIR enables automatic linking using pragmas
#define OPENNURBS_PUBLIC_INSTALL_DIR "C:/Libraries/opennurbs"

#include "C:/Libraries/opennurbs/opennurbs_public.h"

// -------------------------------------
// IGL includes
// -------------------------------------
#include <igl/adjacency_list.h>
#include <igl/average_onto_vertices.h>
#include <igl/barycenter.h>
#include <igl/boundary_facets.h>
#include <igl/boundary_loop.h>
#include <igl/centroid.h>
#include <igl/fast_winding_number.h>
#include <igl/parula.h>
#include <igl/per_corner_normals.h>
#include <igl/per_edge_normals.h>
#include <igl/per_face_normals.h>
#include <igl/per_vertex_normals.h>
#include <igl/principal_curvature.h>
#include <igl/random_points_on_mesh.h>
#include <igl/read_triangle_mesh.h>
#include <igl/triangle_triangle_adjacency.h>

// -------------------------------------
// common headers and namespace
// -------------------------------------
using namespace Eigen;
using namespace std;

#include <Eigen/Core>
#include <map>
#include <numeric>
#include <queue>
#include <random>
#include <set>
#include <unordered_set>
#include <vector>
