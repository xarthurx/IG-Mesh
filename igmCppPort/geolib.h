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

void computeIsoPts(const MatrixXf& V, const MatrixXi& F,
                   const VectorXf& meshScalar, int divN,
                   map<float, MatrixXf>& isoLinePts, bool sorted = true);
}  // namespace GeoLib
