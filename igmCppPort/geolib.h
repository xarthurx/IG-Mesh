#pragma once
#include "stdafx.h"

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

template <typename T>
void solveScalarField(const Matrix<T, -1, -1>& V, const MatrixXi& F,
                      const VectorXi& con_idx, const Vector<T, -1>& con_val,
                      Vector<T, -1>& meshScalar) {
  // boundary edges
  Eigen::MatrixXi E;
  igl::boundary_facets(F, E);

  // find boundiary vertices
  Eigen::VectorXi b, IA, IC;
  igl::unique(E, b, IA, IC);  // all boundary in b

  // List of all vertex indices
  Eigen::VectorXi all, in;
  all = Eigen::VectorXi::LinSpaced(V.rows(), 0, V.rows() - 1);

  igl::setdiff(all, con_idx, in, IA);

  // Slice Laplacian matrix
  Eigen::SparseMatrix<T> L, L_in_in, L_in_b;
  igl::cotmatrix(V, F, L);
  igl::slice(L, in, in, L_in_in);
  igl::slice(L, in, con_idx, L_in_b);

  // slice into meshScalar to assign values
  igl::slice_into(con_val, con_idx, meshScalar);

  // Dirichlet boundary condition from pre-assigned value
  Eigen::Vector<T, -1> bc;  // given boundary value
  igl::slice(meshScalar, con_idx, bc);

  ////Solve PDE
  Eigen::SimplicialLLT<Eigen::SparseMatrix<T>> solver(-L_in_in);
  Eigen::Vector<T, -1> Z_in = solver.solve(L_in_b * bc);

  igl::slice_into(Z_in, in, meshScalar);
};

template <typename T>
void computeIsoPts(const Matrix<T, -1, -1>& V, const MatrixXi& F,
                   const Vector<T, -1>& meshScalar, int divN,
                   map<T, Matrix<T, -1, -1>>& isoLinePts, bool sorted = true) {
  float startBnd{0.0001}, endBnd{0.9999};

  isoLinePts.clear();

  Eigen::MatrixXi E;
  Eigen::VectorXi B;
  igl::edges(F, E);
  igl::boundary_loop(F, B);

  // construct set for querying
  std::set<int> boundLoop;
  for (size_t i = 0; i < B.size(); i++) boundLoop.insert(B[i]);

  //// find & interpolate
  Vector<T, -1> isoValue = Vector<T, -1>::LinSpaced(
      divN, startBnd, endBnd);  // interpolate value for each vertical bars

  map<T, vector<Vector<T, 3>>> isoL;
  for (size_t i = 0; i < isoValue.size(); i++) {
    isoL.insert(make_pair(isoValue[i], vector<Vector<T, 3>>(0)));
  }

  // to record the starting pos of each isoline
  map<T, int> startPtId;

  // extract the points where isoline and mesh edges intersect
  for (size_t i = 0; i < E.rows(); i++) {
    auto idx0 = E.row(i)[0];
    auto idx1 = E.row(i)[1];
    auto x0 = meshScalar[idx0];
    auto x1 = meshScalar[idx1];

    if (x0 > x1) {
      std::swap(x0, x1);
      std::swap(idx0, idx1);
    }

    for (auto& [key, val] : isoL) {
      if (key >= x0 && key <= x1) {
        auto tmpVec = V.row(idx0) +
                      ((key - x0) / (x1 - x0)) * (V.row(idx1) - V.row(idx0));
        val.emplace_back(tmpVec[0], tmpVec[1], tmpVec[2]);

        // record if on boundary
        if (boundLoop.find(idx0) != boundLoop.end() &&
            boundLoop.find(idx1) != boundLoop.end())
          startPtId[key] = val.size() - 1;
      }

      // transform into MatrixXd and export
      Matrix<T, -1, -1> val_mat(val.size(), 3);
      for_each(val.begin(), val.end(), [&](auto& pts) {
        int id = &pts - &val[0];
        val_mat.row(id) = RowVector<T, 3>(pts);
      });

      isoLinePts[key] = val_mat;
    }
  }

  if (sorted) {
    // helper func to find the closest pt in the set
    auto closestPt = [&](const Matrix<T, -1, -1>& pts,
                         const set<int>& existedIdx,
                         RowVector<T, 3>& query_pt) -> int {
      float minD = std::numeric_limits<float>::max();

      int nextId = -1;
      for (size_t i = 0; i < pts.rows(); i++) {
        if (existedIdx.find(i) == existedIdx.end()) {
          float dist = (query_pt - pts.row(i)).norm();
          if (dist < minD) {
            minD = dist;
            nextId = i;
          }
        }
      }

      return nextId;
    };

    // process each isoline
    for (auto& [key, val] : isoLinePts) {
      set<int> addedPtId;

      // prepare data
      vector<RowVector<T, 3>> sortedPolyline{val.row(startPtId[key])};
      addedPtId.insert(startPtId[key]);

      while (addedPtId.size() != val.rows()) {
        int nextPt = closestPt(val, addedPtId, sortedPolyline.back());
        sortedPolyline.push_back(val.row(nextPt));
        addedPtId.insert(nextPt);
      }

      // transfer the sorted list back
      for (size_t i = 0; i < val.rows(); i++) {
        val.row(i) = sortedPolyline.at(i);
      }
    };

    // reverse isolines if needed
    int cnt = 0;
    for (auto it = isoLinePts.begin(); cnt < divN - 1; it++) {
      auto next = std::next(it);

      if ((it->second.row(0) - next->second.row(0)).norm() >
          (it->second.row(0) - next->second.row(next->second.rows() - 1))
              .norm()) {
        next->second.colwise().reverseInPlace();
      }

      cnt++;
    };
  }
}
}  // namespace GeoLib
