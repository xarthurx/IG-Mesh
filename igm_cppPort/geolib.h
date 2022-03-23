#pragma once
#include <igl/adjacency_list.h>
#include <igl/barycenter.h>
#include <igl/boundary_loop.h>
#include <igl/per_face_normals.h>
#include <igl/per_vertex_normals.h>
#include <igl/triangle_triangle_adjacency.h>

#include <Eigen/Core>
#include <map>
#include <queue>
#include <random>
#include <set>
#include <unordered_set>
#include <vector>

using namespace Eigen;
using namespace std;

namespace GeoLib {
  struct MeshStat {
    MatrixXd V, VN, FN, BC;
    MatrixXi F;
    vector<vector<int>> VV;
    vector<int> bndV;

    VectorXi mapV, mapF;

    std::set<int> faceL;
    MatrixXi TT, TTi;

    MeshStat() {
      V.setZero(0, 3);
      F.setZero(0, 3);
      mapV.resize(0);
      mapF.resize(0);

      faceL.clear();
    }

    MeshStat(MatrixXd& Vin, MatrixXi& Fin) : V(Vin), F(Fin) {
      mapV.resize(V.rows());
      mapF.resize(F.rows());

      for (size_t i = 0; i < V.rows(); i++) {
        mapV[i] = i;
      }
      faceL.clear();

      for (size_t i = 0; i < F.rows(); i++) {
        faceL.insert(faceL.end(), i);
        mapF[i] = i;
      }

      igl::per_face_normals(V, F, FN);
      igl::per_vertex_normals(V, F, VN);
      igl::barycenter(V, F, BC);
      igl::triangle_triangle_adjacency(F, TT, TTi);
      igl::adjacency_list(F, VV);
      igl::boundary_loop(F, bndV);
    }

    double costFunc(int f0, int f1) const {
      RowVector3d e01 = BC.row(1) - BC.row(0);
      double nu = FN.row(f0).dot(e01.normalized());
      return (1 + abs(nu)) * (FN.row(f0) - FN.row(f1)).norm();
    }
  };

  void solveScalarField(const MatrixXf& V, const MatrixXi& F,
    const VectorXi& con_idx, const VectorXf& con_value,
    VectorXf& meshScalar);
  void computeIsoPts(const MatrixXf& V, const MatrixXi& F,
    const VectorXf& meshScalar, int divN,
    map<float, MatrixXf>& isoLinePts, bool sorted = true);
}  // namespace GeoLib
