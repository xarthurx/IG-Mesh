#pragma once
#include "stdafx.h"

using namespace Eigen;
using namespace std;

namespace GeoLib {
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
                   const Vector<T, -1>& meshScalar,
                   const Vector<T, -1>& isoValue,
                   map<T, Matrix<T, -1, -1>>& isoLinePts, bool sorted = true) {
  isoLinePts.clear();

  Eigen::MatrixXi E;
  Eigen::VectorXi B;
  igl::edges(F, E);
  igl::boundary_loop(F, B);

  // construct set for querying
  std::set<int> boundLoop;
  for (size_t i = 0; i < B.size(); i++) boundLoop.insert(B[i]);

  map<T, vector<Vector<T, 3>>> isoL;
  for (auto t : isoValue) {
    isoL.insert(make_pair(t, vector<Vector<T, 3>>(0)));
  }

  // to record the starting pos of each isoline
  map<T, int> startPtId;

  // extract the points where isoline and mesh edges intersect. O(nE)
  for (auto& [key, val] : isoL) {
    // iterate through all edges and record pts with the same key value
    for (size_t i = 0; i < E.rows(); i++) {
      auto id0 = E(i, 0);
      auto id1 = E(i, 1);
      auto x0 = meshScalar[id0];
      auto x1 = meshScalar[id1];

      if (x0 > x1) {
        std::swap(x0, x1);
        std::swap(id0, id1);
      }
      // for (auto& [key, val] : isoL) {
      if (key >= x0 && key <= x1) {
        Matrix<T, 1, -1> valPt = (V.row(id0) + V.row(id1)) * 0.5;
        if (x0 != x1)
          valPt =
              V.row(id0) + ((key - x0) / (x1 - x0)) * (V.row(id1) - V.row(id0));

        val.emplace_back(valPt[0], valPt[1], valPt[2]);

        // record if on boundary
        if (boundLoop.find(id0) != boundLoop.end() &&
            boundLoop.find(id1) != boundLoop.end())
          startPtId[key] = val.size() - 1;
      }
    }

    Matrix<T, -1, -1> val_mat;
    val_mat.resize(val.size(), 3);

    for_each(val.begin(), val.end(), [&](auto& pts) {
      size_t id = &pts - &val[0];
      val_mat.row(id) = pts.transpose();
    });

    // if only one points, then remove it.
    if (val.size() == 1) {
      val_mat.resize(0, 0);
    }

    isoLinePts[key] = val_mat;
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
      if (val.rows() == 0) continue;

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

    // collect valid isolines
    vector<T> validIso(0);
    for (auto& [key, val] : isoLinePts) {
      if (val.rows() != 0) validIso.push_back(key);
    }

    // reverse isolines if needed
    if (validIso.size() > 1) {
      for (size_t i = 0; i < validIso.size() - 1; i++) {
        auto& key0 = validIso[i];
        auto& key1 = validIso[i + 1];

        auto& ln0 = isoLinePts[key0];
        auto& ln1 = isoLinePts[key1];

        if ((ln0.row(0) - ln1.row(0)).norm() >
            (ln0.row(0) - ln1.row(ln1.rows() - 1)).norm()) {
          ln1.colwise().reverseInPlace();
        }
      }
    }
  }
}
}  // namespace GeoLib
