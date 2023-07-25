#pragma once
#include "stdafx.h"

namespace GeoLib {
// solve the constrained scalar field of a mesh
template <typename T>
void solveScalarField(const Matrix<T, -1, -1>& V, const MatrixXi& F,
                      const VectorXi& con_idx, const Vector<T, -1>& con_val,
                      Vector<T, -1>& meshScalar);


// extract isolines from a mesh based on the given scalar field
template <typename T>
void computeIsoPts(const Matrix<T, -1, -1>& V, const MatrixXi& F,
                   const Vector<T, -1>& meshScalar,
                   const Vector<T, -1>& isoValue,
                   map<T, Matrix<T, -1, -1>>& isoLinePts, bool sorted = true);

}  // namespace GeoLib
