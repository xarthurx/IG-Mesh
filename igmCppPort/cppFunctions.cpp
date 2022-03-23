#include "cppFunctions.h"

#include "geolib.h"

using RowMajMatXd = Matrix<double, Dynamic, Dynamic, RowMajor>;
using RowMajMatXf = Matrix<float, Dynamic, Dynamic, RowMajor>;
using RowMajMatXi = Matrix<int, Dynamic, Dynamic, RowMajor>;

// helper function
template <typename T>
void convertArrayToEigenXt(T* inputArray, int sz,
                           Eigen::Matrix<T, Dynamic, Dynamic>& outputEigen) {
  int cnt = 0;
  outputEigen.resize(sz, 3);

  while (cnt != sz) {
    outputEigen(cnt, 0) = inputArray[cnt * 3];
    outputEigen(cnt, 1) = inputArray[cnt * 3 + 1];
    outputEigen(cnt, 2) = inputArray[cnt * 3 + 2];
    cnt++;
  }
}

// void convertArrayToEigenXf(float* inputArray, int sz,
//                           Eigen::MatrixXf& outputEigen) {
//  int cnt = 0;
//  outputEigen.resize(sz, 3);
//
//  while (cnt != sz) {
//    outputEigen(cnt, 0) = inputArray[cnt * 3];
//    outputEigen(cnt, 1) = inputArray[cnt * 3 + 1];
//    outputEigen(cnt, 2) = inputArray[cnt * 3 + 2];
//    cnt++;
//  }
//}

// void convertArrayToEigenXi(int* inputArray, int sz,
//                           Eigen::MatrixXi& outputEigen) {
//  int cnt = 0;
//  outputEigen.resize(sz, 3);
//
//  while (cnt != sz) {
//    outputEigen.row(cnt) << inputArray[cnt * 3], inputArray[cnt * 3 + 1],
//        inputArray[cnt * 3 + 2];
//    cnt++;
//  }
//}

void convertONstructToEigen(ON_3dPointArray& mV,
                            ON_SimpleArray<ON_MeshFace>& mF, MatrixXd& matV,
                            MatrixXi& matF) {
  matV.resize(mV.Count(), 3);
  for (size_t i = 0; i < mV.Count(); i++) {
    matV.row(i) << mV[i].x, mV[i].y, mV[i].z;
  }

  matF.resize(mF.Count(), 3);
  for (size_t i = 0; i < mF.Count(); i++) {
    matF.row(i) << mF[i].vi[0], mF[i].vi[1], mF[i].vi[2];
  }
}

template <typename T>
void convertEigenToON_Points(const Matrix<T, Dynamic, Dynamic>& matP,
                             ON_3dPointArray* P) {
  for (size_t i = 0; i < matP.rows(); i++) {
    P->Append(ON_3dPoint(matP(i, 0), matP(i, 1), matP(i, 2)));
  }
}

// void convertEigenToON_Points(const MatrixXd& matP, ON_3dPointArray* P) {
//  for (size_t i = 0; i < matP.rows(); i++) {
//    P->Append(ON_3dPoint(matP(i, 0), matP(i, 1), matP(i, 2)));
//  }
//}
//

template <typename T>
void convertEigenToON_Vectors(const Matrix<T, Dynamic, Dynamic>& matV,
                              ON_3dVectorArray* V) {
  for (size_t i = 0; i < matV.rows(); i++) {
    V->Append(ON_3dVector(matV(i, 0), matV(i, 1), matV(i, 2)));
  }
}

template <typename T>
void convertEigenVecToON_Array(const Vector<T, Dynamic>& vecV,
                               ON_SimpleArray<T>* P) {
  P->Append(vecV.size(), vecV.data());
}

void IGM_adjacency_list(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  convertArrayToEigenXt(F, nF, eigenF);

  vector<vector<int>> lst;
  igl::adjacency_list(eigenF, lst);

  vector<int> transferLst(0);
  for_each(lst.begin(), lst.end(), [&](vector<int>& vec) {
    // size as indicator
    transferLst.push_back(vec.size());
    // copy all values
    std::copy(vec.begin(), vec.end(), std::back_inserter(transferLst));
  });

  std::copy(transferLst.begin(), transferLst.end(), adjLst);

  // the total # of neighbouring vert + the # of vert (as indicator of each
  // vector's size)
  sz = lst.size() + std::accumulate(lst.begin(), lst.end(), (size_t)0,
                                    [&](int res, vector<int>& vec) {
                                      return res + vec.size();
                                    });
}

void IGM_vertex_triangle_adjacency(int nV, int* F, int nF, int* adjVF,
                                   int* adjVFI, int& sz) {
  Eigen::MatrixXi matF;
  convertArrayToEigenXt(F, nF, matF);

  vector<vector<int>> VF, VFI;
  igl::vertex_triangle_adjacency(nV, matF, VF, VFI);

  vector<int> tmpVF(0), tmpVFI(0);
  for_each(VF.begin(), VF.end(), [&](vector<int>& vec) {
    tmpVF.push_back(vec.size());  // size as indicator
    std::copy(vec.begin(), vec.end(),
              std::back_inserter(tmpVF));  // copy all values
  });

  std::copy(tmpVF.begin(), tmpVF.end(), adjVF);

  for_each(VFI.begin(), VFI.end(), [&](vector<int>& vec) {
    tmpVF.push_back(vec.size());  // size as indicator
    std::copy(vec.begin(), vec.end(),
              std::back_inserter(tmpVFI));  // copy all values
  });

  std::copy(tmpVFI.begin(), tmpVFI.end(), adjVFI);

  // the total # of neighbouring vert + the # of vert (as indicator of each
  // vector's size)
  sz = VF.size() + std::accumulate(VF.begin(), VF.end(), (size_t)0,
                                   [&](int res, vector<int>& vec) {
                                     return res + vec.size();
                                   });  // same for VF and VFI
}

void IGM_triangle_triangle_adjacency(int* F, int nF, int* adjTT, int* adjTTI) {
  // if (pMesh->HasDoublePrecisionVertices()) {
  //  ON_3dPointArray& V = pMesh->m_dV;
  //}

  MatrixXi matF;
  convertArrayToEigenXt(F, nF, matF);
  MatrixXi matTT, matTTI;

  igl::triangle_triangle_adjacency(matF, matTT, matTTI);

  RowMajMatXi::Map(adjTT, matTT.rows(), matTT.cols()) = matTT;
  RowMajMatXi::Map(adjTTI, matTTI.rows(), matTTI.cols()) = matTTI;
}

void IGM_boundary_loop(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  convertArrayToEigenXt(F, nF, eigenF);

  vector<vector<int>> lst;
  igl::boundary_loop(eigenF, lst);

  vector<int> transferLst(0);
  for_each(lst.begin(), lst.end(), [&](vector<int>& vec) {
    // size as indicator
    transferLst.push_back(vec.size());
    // copy all values
    std::copy(vec.begin(), vec.end(), std::back_inserter(transferLst));
  });

  std::copy(transferLst.begin(), transferLst.end(), adjLst);

  // the total # of boundary loops + the # of vert (as indicator of each
  // vector's size)
  sz = lst.size() + std::accumulate(lst.begin(), lst.end(), (size_t)0,
                                    [&](int res, vector<int>& vec) {
                                      return res + vec.size();
                                    });
}

void IGM_boundary_facet(int* F, int nF, int* edge, int* triIdx, int& sz) {
  // convert mesh
  MatrixXi matF;
  convertArrayToEigenXt(F, nF, matF);

  // compute
  MatrixXi bF;
  VectorXi J, K;
  igl::boundary_facets(matF, bF, J, K);

  // convert back
  RowMajMatXi::Map(edge, bF.rows(), bF.cols()) = bF;
  VectorXi::Map(triIdx, J.size()) = J;
  sz = J.size();
}

// void IGM_barycenter(float* V, int nV, int* F, int nF, float* BC)
//{
//  // convert mesh
//  MatrixXf matV;
//  MatrixXi matF;
//  convertArrayToEigenXf(V, nV, matV);
//  convertArrayToEigenXi(F, nF, matF);
//
//  // do the igl calculation
//  MatrixXf matBC;
//  igl::barycenter(matV, matF, matBC);
//
//  // convert back to arrays
//  RowMajMatXf::Map(BC, matBC.rows(), matBC.cols()) = matBC;
//}

void IGM_centroid(ON_Mesh* pMesh, ON_3dPointArray* c) {
  // convert mesh
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  Vector3d cen;

  // compute
  igl::centroid(matV, matF, cen);

  // one-item array due to limitation of wrappers
  convertEigenToON_Points(MatrixXd(cen.transpose()), c);
}

void IGM_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC) {
  // convert mesh
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  // call func
  MatrixXd matBC;
  igl::barycenter(matV, matF, matBC);

  // convert back
  convertEigenToON_Points(matBC, BC);
}

void IGM_vert_normals(ON_Mesh* pMesh, ON_3dPointArray* VN) {
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matVN;
  igl::per_vertex_normals(matV, matF, matVN);

  convertEigenToON_Points(matVN, VN);
}

void IGM_face_normals(ON_Mesh* pMesh, ON_3dPointArray* FN) {
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matFN;
  igl::per_face_normals(matV, matF, matFN);

  convertEigenToON_Points(matFN, FN);
}

void IGM_corner_normals(ON_Mesh* pMesh, double threshold_deg,
                        ON_3dPointArray* CN) {
  // convert mesh
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  // compute per-corner-normal
  MatrixXd matCN;
  igl::per_corner_normals(matV, matF, threshold_deg, matCN);

  // convert back
  convertEigenToON_Points(matCN, CN);
}

void IGM_edge_normals(float* V, int nV, int* F, int nF, int weightingType,
                      float* EN, int* EI, int* EMAP, int& sz) {
  // convert mesh
  MatrixXf matV;
  MatrixXi matF;
  convertArrayToEigenXt(V, nV, matV);
  convertArrayToEigenXt(F, nF, matF);

  // compute per-edge-normal
  MatrixXf matEN;
  MatrixXf matFN;
  MatrixXi matEI;
  VectorXi vecEMAP;
  igl::per_edge_normals(
      matV, matF, static_cast<igl::PerEdgeNormalsWeightingType>(weightingType),
      matEN, matEI, vecEMAP);

  // convert back
  RowMajMatXf::Map(EN, matEN.rows(), matEN.cols()) = matEN;
  RowMajMatXi::Map(EI, matEI.rows(), matEI.cols()) = matEI;
  VectorXi::Map(EMAP, vecEMAP.size()) = vecEMAP;
  sz = matEI.rows();
}

// RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF, int* con_idx,
                       float* con_value, int numCon, int divN, float* isoLnPts,
                       int* numPtsPerLst) {
  // size of 'numPtsPerLst'  =  divN;  "numPtsPerLst" contains the # of pts of
  // "isoLnPts' in each isoline

  // convert mesh
  MatrixXf eigenV;
  MatrixXi eigenF;
  convertArrayToEigenXt(V, nV, eigenV);
  convertArrayToEigenXt(F, nF, eigenF);

  // convert constraints
  VectorXi conIdx(numCon);
  VectorXf conVal(numCon);

  for (size_t i = 0; i < numCon; i++) {
    conIdx[i] = *(con_idx + i);
    conVal[i] = *(con_value + i);
  }

  // solve scalar field
  VectorXf meshScalar(eigenV.rows());
  GeoLib::solveScalarField(eigenV, eigenF, conIdx, conVal, meshScalar);

  // extract isolines
  map<float, MatrixXf> tmpIsoPts;
  GeoLib::computeIsoPts(eigenV, eigenF, meshScalar, divN, tmpIsoPts);

  // write data back to the c# pointer arrary
  vector<int> transferNumPtPerLst;
  vector<float> transferIsoLnCollection(0);
  for (auto const& [key, val] : tmpIsoPts) {
    vector<float> transferIsoLn(val.rows() * 3);

    // isoline points
    for (size_t i = 0; i < val.rows(); i++) {
      transferIsoLnCollection.emplace_back(val(i, 0));
      transferIsoLnCollection.emplace_back(val(i, 1));
      transferIsoLnCollection.emplace_back(val(i, 2));
    }

    // number of pts per isoline
    transferNumPtPerLst.push_back(val.rows());
  }

  std::copy(transferIsoLnCollection.begin(), transferIsoLnCollection.end(),
            isoLnPts);
  std::copy(transferNumPtPerLst.begin(), transferNumPtPerLst.end(),
            numPtsPerLst);
}

// RH_C_FUNCTION
void computeLaplacian(float* V, int nV, int* F, int nF, int* con_idx,
                      float* con_value, int numCon, float* laplacianValue) {
  // size of 'numPtsPerLst'  =  divN;  "numPtsPerLst" contains the # of pts of
  // "isoLnPts' in each isoline

  // convert mesh
  MatrixXf eigenV;
  MatrixXi eigenF;
  convertArrayToEigenXt(V, nV, eigenV);
  convertArrayToEigenXt(F, nF, eigenF);

  // convert constraints
  VectorXi conIdx(numCon);
  VectorXf conVal(numCon);

  for (size_t i = 0; i < numCon; i++) {
    conIdx[i] = *(con_idx + i);
    conVal[i] = *(con_value + i);
  }

  // solve scalar field
  VectorXf meshScalar(eigenV.rows());
  GeoLib::solveScalarField(eigenV, eigenF, conIdx, conVal, meshScalar);

  // transfer data back
  VectorXf meshScalarFloat = meshScalar.cast<float>();
  Eigen::VectorXf::Map(laplacianValue, meshScalarFloat.rows()) =
      meshScalarFloat;
}

void IGM_random_point_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* B,
                              ON_SimpleArray<int>* FI) {
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matP;
  VectorXi faceI;
  igl::random_points_on_mesh(N, matV, matF, matP, faceI);

  // convert from P on a plane into Euclidian space
  MatrixXd samples(matP.rows(), 3);
  for (int i = 0; i < matP.rows(); i++) {
    samples.row(i) = matP(i, 0) * matV.row(matF(faceI(i), 0)) +
                     matP(i, 1) * matV.row(matF(faceI(i), 1)) +
                     matP(i, 2) * matV.row(matF(faceI(i), 2));
  }

  convertEigenToON_Points(samples, B);
  convertEigenVecToON_Array(faceI, FI);
}

void IGM_principal_curvature(ON_Mesh* pMesh, unsigned r, ON_3dPointArray* PD1,
                             ON_3dPointArray* PD2, ON_SimpleArray<double>* PV1,
                             ON_SimpleArray<double>* PV2) {
  MatrixXd matV;
  MatrixXi matF;
  convertONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd mPD1, mPD2;
  VectorXd mPV1, mPV2;
  igl::principal_curvature(matV, matF, mPD1, mPD2, mPV1, mPV2, r);

  convertEigenToON_Points(mPD1, PD1);
  convertEigenToON_Points(mPD2, PD2);

  convertEigenVecToON_Array(mPV1, PV1);
  convertEigenVecToON_Array(mPV2, PV2);
}