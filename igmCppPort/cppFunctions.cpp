#include "cppFunctions.h"

#include "geolib.h"

using RowMajMatXd = Matrix<double, Dynamic, Dynamic, RowMajor>;
using RowMajMatXf = Matrix<float, Dynamic, Dynamic, RowMajor>;
using RowMajMatXi = Matrix<int, Dynamic, Dynamic, RowMajor>;

// helper function
template <typename T>
void cvtArrayToEigenXt(T* inputArray, int sz,
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

void cvtOn_PtArrayToEigen(ON_3dPointArray* V, MatrixXd& matV) {
  matV.resize(V->Count(), 3);
  for (size_t i = 0; i < V->Count(); i++) {
    matV.row(i) << V->At(i)->x, V->At(i)->y, V->At(i)->z;
  }
}

void cvtONstructToEigen(ON_3dPointArray& mV, ON_SimpleArray<ON_MeshFace>& mF,
                        MatrixXd& matV, MatrixXi& matF) {
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
void cvtON_ArrayToEigenV(ON_SimpleArray<T>* onArray, Vector<T, -1>& vec) {
  // vec = Vector<T, -1>(onArray->Array());
  vec = Vector<T, -1>::Map(onArray->Array(), onArray->Count());
}
template <typename T>
void cvtEigenVToArray(Vector<T, -1>& vec, ON_SimpleArray<T>* onArray) {
  for (size_t i = 0; i < vec.size(); i++) {
    onArray->Append(vec[i]);
  }
}

template <typename T>
void cvtEigenToON_Points(const Matrix<T, Dynamic, Dynamic>& matP,
                         ON_3dPointArray* P) {
  for (size_t i = 0; i < matP.rows(); i++) {
    P->Append(ON_3dPoint(matP(i, 0), matP(i, 1), matP(i, 2)));
  }
}

template <typename T>
void cvtEigenToON_Vectors(const Matrix<T, Dynamic, Dynamic>& matV,
                          ON_3dVectorArray* V) {
  for (size_t i = 0; i < matV.rows(); i++) {
    V->Append(ON_3dVector(matV(i, 0), matV(i, 1), matV(i, 2)));
  }
}

void cvtEigenToON_ArrayInt(const MatrixXi& matF, ON_SimpleArray<int>* F) {
  for (size_t i = 0; i < matF.rows(); i++) {
    int tmp[3]{matF(i, 0), matF(i, 1), matF(i, 2)};
    F->Append(3, tmp);
  }
}

template <typename T>
void cvtEigenVecToON_Array(const Vector<T, Dynamic>& vecV,
                           ON_SimpleArray<T>* P) {
  P->Append(vecV.size(), vecV.data());
}

// bool IGM_read_triangle_mesh(char* filename, ON_3dPointArray* V,
//                            ON_SimpleArray<int>* F) {
//  MatrixXd matV;
//  MatrixXi matF;
//  auto res = igl::read_triangle_mesh(filename, matV, matF);
//
//  if (res) {
//    cvtEigenToON_Points(matV, V);
//    cvtEigenToON_ArrayInt(matF, F);
//  }
//
//  return res;
//}

void IGM_read_triangle_mesh(char* filename, ON_Mesh* pMesh) {
  MatrixXd matV;
  MatrixXi matF;
  igl::read_triangle_mesh(filename, matV, matF);

  for (size_t i = 0; i < matV.rows(); i++) {
    pMesh->SetVertex(i, ON_3dPoint(matV(i, 0), matV(i, 1), matV(i, 2)));
  }
  for (size_t i = 0; i < matF.rows(); i++) {
    pMesh->SetTriangle(i, matF(i, 0), matF(i, 1), matF(i, 2));
  }
}

void IGM_centroid(ON_Mesh* pMesh, ON_3dPointArray* c) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  Vector3d cen;

  // compute
  igl::centroid(matV, matF, cen);

  // one-item array due to limitation of wrappers
  cvtEigenToON_Points(MatrixXd(cen.transpose()), c);
}

void IGM_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  // call func
  MatrixXd matBC;
  igl::barycenter(matV, matF, matBC);

  // cvt back
  cvtEigenToON_Points(matBC, BC);
}

void IGM_adjacency_list(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  cvtArrayToEigenXt(F, nF, eigenF);

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
  cvtArrayToEigenXt(F, nF, matF);

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
  cvtArrayToEigenXt(F, nF, matF);
  MatrixXi matTT, matTTI;

  igl::triangle_triangle_adjacency(matF, matTT, matTTI);

  RowMajMatXi::Map(adjTT, matTT.rows(), matTT.cols()) = matTT;
  RowMajMatXi::Map(adjTTI, matTTI.rows(), matTTI.cols()) = matTTI;
}

void IGM_boundary_loop(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  cvtArrayToEigenXt(F, nF, eigenF);

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
  // cvt mesh
  MatrixXi matF;
  cvtArrayToEigenXt(F, nF, matF);

  // compute
  MatrixXi bF;
  VectorXi J, K;
  igl::boundary_facets(matF, bF, J, K);

  // cvt back
  RowMajMatXi::Map(edge, bF.rows(), bF.cols()) = bF;
  VectorXi::Map(triIdx, J.size()) = J;
  sz = J.size();
}

void IGM_vert_normals(ON_Mesh* pMesh, ON_3dPointArray* VN) {
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matVN;
  igl::per_vertex_normals(matV, matF, matVN);

  cvtEigenToON_Points(matVN, VN);
}

void IGM_face_normals(ON_Mesh* pMesh, ON_3dPointArray* FN) {
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matFN;
  igl::per_face_normals(matV, matF, matFN);

  cvtEigenToON_Points(matFN, FN);
}

void IGM_corner_normals(ON_Mesh* pMesh, double threshold_deg,
                        ON_3dPointArray* CN) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  // compute per-corner-normal
  MatrixXd matCN;
  igl::per_corner_normals(matV, matF, threshold_deg, matCN);

  // cvt back
  cvtEigenToON_Points(matCN, CN);
}

void IGM_edge_normals(float* V, int nV, int* F, int nF, int weightingType,
                      float* EN, int* EI, int* EMAP, int& sz) {
  // cvt mesh
  MatrixXf matV;
  MatrixXi matF;
  cvtArrayToEigenXt(V, nV, matV);
  cvtArrayToEigenXt(F, nF, matF);

  // compute per-edge-normal
  MatrixXf matEN;
  MatrixXf matFN;
  MatrixXi matEI;
  VectorXi vecEMAP;
  igl::per_edge_normals(
      matV, matF, static_cast<igl::PerEdgeNormalsWeightingType>(weightingType),
      matEN, matEI, vecEMAP);

  // cvt back
  RowMajMatXf::Map(EN, matEN.rows(), matEN.cols()) = matEN;
  RowMajMatXi::Map(EI, matEI.rows(), matEI.cols()) = matEI;
  VectorXi::Map(EMAP, vecEMAP.size()) = vecEMAP;
  sz = matEI.rows();
}

void IGM_remapFtoV(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  VectorXd scalarVal;
  cvtON_ArrayToEigenV(val, scalarVal);

  // compute data
  VectorXd vecSV;
  igl::average_onto_vertices(matV, matF, scalarVal, vecSV);

  // send back
  cvtEigenVecToON_Array(vecSV, res);
}

// RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF, int* con_idx,
                       float* con_value, int numCon, int divN, float* isoLnPts,
                       int* numPtsPerLst) {
  // size of 'numPtsPerLst'  =  divN;  "numPtsPerLst" contains the # of pts of
  // "isoLnPts' in each isoline

  // cvt mesh
  MatrixXf eigenV;
  MatrixXi eigenF;
  cvtArrayToEigenXt(V, nV, eigenV);
  cvtArrayToEigenXt(F, nF, eigenF);

  // cvt constraints
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

  // cvt mesh
  MatrixXf eigenV;
  MatrixXi eigenF;
  cvtArrayToEigenXt(V, nV, eigenV);
  cvtArrayToEigenXt(F, nF, eigenF);

  // cvt constraints
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
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matP;
  VectorXi faceI;
  igl::random_points_on_mesh(N, matV, matF, matP, faceI);

  // cvt from P on a plane into Euclidian space
  MatrixXd samples(matP.rows(), 3);
  for (int i = 0; i < matP.rows(); i++) {
    samples.row(i) = matP(i, 0) * matV.row(matF(faceI(i), 0)) +
                     matP(i, 1) * matV.row(matF(faceI(i), 1)) +
                     matP(i, 2) * matV.row(matF(faceI(i), 2));
  }

  cvtEigenToON_Points(samples, B);
  cvtEigenVecToON_Array(faceI, FI);
}

void IGM_principal_curvature(ON_Mesh* pMesh, unsigned r, ON_3dPointArray* PD1,
                             ON_3dPointArray* PD2, ON_SimpleArray<double>* PV1,
                             ON_SimpleArray<double>* PV2) {
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd mPD1, mPD2;
  VectorXd mPV1, mPV2;
  igl::principal_curvature(matV, matF, mPD1, mPD2, mPV1, mPV2, r);

  cvtEigenToON_Points(mPD1, PD1);
  cvtEigenToON_Points(mPD2, PD2);

  cvtEigenVecToON_Array(mPV1, PV1);
  cvtEigenVecToON_Array(mPV2, PV2);
}

void IGM_fast_winding_number(ON_Mesh* pMesh, ON_SimpleArray<double>* Q,
                             ON_SimpleArray<double>* W) {
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matQ;
  cvtArrayToEigenXt(Q->Array(), Q->Count() / 3, matQ);

  VectorXd vecW;
  igl::fast_winding_number(matV, matF, matQ, vecW);

  cvtEigenVecToON_Array(vecW, W);
}

void IGM_signed_distance(ON_Mesh* pMesh, ON_SimpleArray<double>* Q, int type,
                         ON_SimpleArray<double>* S, ON_SimpleArray<int>* I,
                         ON_3dPointArray* C) {
  MatrixXd matV;
  MatrixXi matF;
  cvtONstructToEigen(pMesh->m_dV, pMesh->m_F, matV, matF);

  MatrixXd matQ;
  cvtArrayToEigenXt(Q->Array(), Q->Count() / 3, matQ);

  // make sure "type" within range.
  if (type < 1 || type > 4) type = 4;

  VectorXd vecS;
  VectorXi vecI;
  MatrixXd matC;
  MatrixXd matN;  // tmp variable for now.

  igl::signed_distance(matQ, matV, matF, (igl::SignedDistanceType)type, vecS,
                       vecI, matC, matN);

  // convert data back
  cvtEigenVToArray(vecS, S);
  cvtEigenVToArray(vecI, I);
  cvtEigenToON_Points(matC, C);
}
