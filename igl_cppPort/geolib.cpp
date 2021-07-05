#include "geolib.h"

#include <igl/boundary_facets.h>
#include <igl/cotmatrix.h>
#include <igl/edges.h>
#include <igl/setdiff.h>
#include <igl/slice_into.h>
#include <igl/unique.h>

void GeoLib::solveScalarField(const MatrixXd& V, const MatrixXi& F,
                              const VectorXi& con_idx,
                              const VectorXd& con_value, VectorXd& meshScalar) {
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
  Eigen::SparseMatrix<double> L, L_in_in, L_in_b;
  igl::cotmatrix(V, F, L);
  igl::slice(L, in, in, L_in_in);
  igl::slice(L, in, con_idx, L_in_b);

  // slice into meshScalar to assign values
  igl::slice_into(con_value, con_idx, meshScalar);

  // Dirichlet boundary condition from pre-assigned value
  Eigen::VectorXd bc;  // given boundary value
  igl::slice(meshScalar, con_idx, bc);

  ////Solve PDE
  Eigen::SimplicialLLT<Eigen::SparseMatrix<double>> solver(-L_in_in);
  Eigen::VectorXd Z_in = solver.solve(L_in_b * bc);

  igl::slice_into(Z_in, in, meshScalar);
}

void GeoLib::computeIsoPts(const MatrixXd& V, const MatrixXi& F,
                           const VectorXd& meshScalar, int divN,
                           map<double, MatrixXd>& isoLinePts, bool sorted) {
  double startBnd = 0.0001;
  double endBnd = 0.9999;

  isoLinePts.clear();

  Eigen::MatrixXi E;
  Eigen::VectorXi B;
  igl::edges(F, E);
  igl::boundary_loop(F, B);

  // construct set for querying
  std::set<int> boundLoop;
  for (size_t i = 0; i < B.size(); i++) boundLoop.insert(B[i]);

  //// find & interpolate
  VectorXd isoValue = Eigen::VectorXd::LinSpaced(
      divN, startBnd, endBnd);  // interpolate value for each vertical bars

  map<double, vector<Vector3d>> isoL;
  for (size_t i = 0; i < isoValue.size(); i++) {
    isoL.insert(make_pair(isoValue[i], vector<Vector3d>(0)));
  }

  // to record the starting pos of each isoline
  map<double, int> startPtId;

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
        // auto tmpVec = (V.row(idx1) - V.row(idx0)).cast<double>();
        auto tmpVec = V.row(idx0) +
                      ((key - x0) / (x1 - x0)) * (V.row(idx1) - V.row(idx0));
        val.emplace_back(tmpVec[0], tmpVec[1], tmpVec[2]);

        // record if on boundary
        if (boundLoop.find(idx0) != boundLoop.end() &&
            boundLoop.find(idx1) != boundLoop.end())
          startPtId[key] = val.size() - 1;
      }

      // transform into MatrixXd and export
      MatrixXd val_mat(val.size(), 3);
      for_each(val.begin(), val.end(), [&](auto& pts) {
        int id = &pts - &val[0];
        val_mat.row(id) = RowVector3d(pts);
      });

      isoLinePts[key] = val_mat;
    }
  }

  if (sorted) {
    // helper func to find the closest pt in the set
    auto closestPt = [&](const MatrixXd& pts, const set<int>& existedIdx,
                         RowVector3d& query_pt) -> int {
      double minD = std::numeric_limits<double>::max();

      int nextId = -1;
      for (size_t i = 0; i < pts.rows(); i++) {
        if (existedIdx.find(i) == existedIdx.end()) {
          double dist = (query_pt - pts.row(i)).norm();
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
      vector<RowVector3d> sortedPolyline{val.row(startPtId[key])};
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

      // spdlog::info("original dist: {}", (it->second.row(0) -
      // next->second.row(0)).norm()); spdlog::info("new dist: {}",
      // (it->second.row(0) - next->second.row(next->second.rows() -
      // 1)).norm());

      if ((it->second.row(0) - next->second.row(0)).norm() >
          (it->second.row(0) - next->second.row(next->second.rows() - 1))
              .norm()) {
        next->second.colwise().reverseInPlace();
      }

      cnt++;
    };
  }
}
