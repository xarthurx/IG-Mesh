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

void cvtMeshToEigen(ON_Mesh* pMesh, MatrixXd& matV, MatrixXi& matF) {
  auto& mV = pMesh->m_dV;
  if (!pMesh->HasDoublePrecisionVertices()) {
    mV = pMesh->m_V;
  }

  matV.resize(mV.Count(), 3);
  for (size_t i = 0; i < mV.Count(); i++) {
    matV.row(i) << mV[i].x, mV[i].y, mV[i].z;
  }

  auto& mF = pMesh->m_F;
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
void cvtEigenVToON_Array(Vector<T, -1>& vec, ON_SimpleArray<T>* onArray) {
  onArray->Append(vec.size(), vec.data());
  // for (size_t i = 0; i < vec.size(); i++) {
  //  onArray->Append(vec[i]);
  //}
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

bool IGM_write_triangle_mesh(char* filename, ON_Mesh* pMesh) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  return igl::write_triangle_mesh(filename, matV, matF);
}

void IGM_centroid(ON_Mesh* pMesh, ON_SimpleArray<double>* c) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  Vector3d cen;

  // compute
  igl::centroid(matV, matF, cen);

  c->Append(cen[0]);
  c->Append(cen[1]);
  c->Append(cen[2]);
}

void IGM_barycenter(ON_Mesh* pMesh, ON_3dPointArray* BC) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

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
  cvtMeshToEigen(pMesh, matV, matF);

  MatrixXd matVN;
  igl::per_vertex_normals(matV, matF, matVN);

  cvtEigenToON_Points(matVN, VN);
}

void IGM_face_normals(ON_Mesh* pMesh, ON_3dPointArray* FN) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  MatrixXd matFN;
  igl::per_face_normals(matV, matF, matFN);

  cvtEigenToON_Points(matFN, FN);
}

void IGM_corner_normals(ON_Mesh* pMesh, double threshold_deg,
                        ON_3dPointArray* CN) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

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
  cvtMeshToEigen(pMesh, matV, matF);

  VectorXd scalarVal;
  cvtON_ArrayToEigenV(val, scalarVal);

  // compute data
  VectorXd vecSV;
  igl::average_onto_vertices(matV, matF, scalarVal, vecSV);

  // send back
  cvtEigenVToON_Array(vecSV, res);
}

void IGM_remapVtoF(ON_Mesh* pMesh, ON_SimpleArray<double>* val,
                   ON_SimpleArray<double>* res) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  VectorXd scalarVal;
  cvtON_ArrayToEigenV(val, scalarVal);

  // compute data
  VectorXd vecSF;
  igl::average_onto_faces(matF, scalarVal, vecSF);

  // send back
  cvtEigenVToON_Array(vecSF, res);
}

void IGM_constrained_scalar(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                            ON_SimpleArray<double>* con_val,
                            ON_SimpleArray<double>* meshScal) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  // cvt constraints
  VectorXi conIdx;
  VectorXd conVal, isoVal;
  cvtON_ArrayToEigenV(con_idx, conIdx);
  cvtON_ArrayToEigenV(con_val, conVal);

  // solve scalar field
  VectorXd meshScalar(matV.rows());
  GeoLib::solveScalarField(matV, matF, conIdx, conVal, meshScalar);
  cvtEigenVToON_Array(meshScalar, meshScal);
}

void IGM_extract_isoline_from_scalar(ON_Mesh* pMesh,
                                     ON_SimpleArray<double>* meshS,
                                     ON_SimpleArray<double>* iso_t,
                                     ON_SimpleArray<ON_3dPointArray*>* isoP) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  VectorXd meshScalar, isoVal;
  cvtON_ArrayToEigenV(meshS, meshScalar);
  cvtON_ArrayToEigenV(iso_t, isoVal);

  // extract isolines
  std::map<double, MatrixXd> tmpIsoPts;
  GeoLib::computeIsoPts(matV, matF, meshScalar, isoVal, tmpIsoPts);

  // use the sorted list
  for (auto const& [key, val] : tmpIsoPts) {
    auto& tmpL = isoP->AppendNew();
    tmpL = new ON_3dPointArray();
    for (size_t i = 0; i < val.rows(); i++) {
      tmpL->Append(ON_3dPoint(val(i, 0), val(i, 1), val(i, 2)));
    }
  }
}

void IGM_extract_isoline(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                         ON_SimpleArray<double>* con_val,
                         ON_SimpleArray<double>* iso_t,
                         ON_SimpleArray<ON_3dPointArray*>* isoP,
                         ON_SimpleArray<double>* meshS) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  // cvt constraints
  VectorXi conIdx;
  VectorXd conVal, isoVal;
  cvtON_ArrayToEigenV(con_idx, conIdx);
  cvtON_ArrayToEigenV(con_val, conVal);
  cvtON_ArrayToEigenV(iso_t, isoVal);

  // solve scalar field
  VectorXd meshScalar(matV.rows());
  GeoLib::solveScalarField(matV, matF, conIdx, conVal, meshScalar);
  cvtEigenVToON_Array(meshScalar, meshS);

  // extract isolines
  std::map<double, MatrixXd> tmpIsoPts;
  GeoLib::computeIsoPts(matV, matF, meshScalar, isoVal, tmpIsoPts);

  // use the sorted list
  for (auto const& [key, val] : tmpIsoPts) {
    auto& tmpL = isoP->AppendNew();
    tmpL = new ON_3dPointArray();
    for (size_t i = 0; i < val.rows(); i++) {
      tmpL->Append(ON_3dPoint(val(i, 0), val(i, 1), val(i, 2)));
    }
  }
}

void IGM_laplacian(ON_Mesh* pMesh, ON_SimpleArray<int>* con_idx,
                   ON_SimpleArray<double>* con_val,
                   ON_SimpleArray<double>* laplacianValue) {
  // cvt mesh
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  // cvt cons
  VectorXi vecConIdx;
  VectorXd vecConVal;
  cvtON_ArrayToEigenV(con_idx, vecConIdx);
  cvtON_ArrayToEigenV(con_val, vecConVal);

  // solve scalar field
  VectorXd lapVal;
  GeoLib::solveScalarField(matV, matF, vecConIdx, vecConVal, lapVal);

  // transfer data back
  cvtEigenVToON_Array(lapVal, laplacianValue);
}

void IGM_random_point_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* P,
                              ON_SimpleArray<int>* FI) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  MatrixXd B, matP;
  VectorXi faceI;

  igl::random_points_on_mesh(N, matV, matF, B, faceI, matP);

  cvtEigenToON_Points(matP, P);
  cvtEigenVToON_Array(faceI, FI);
}

void IGM_blue_noise_sampling_on_mesh(ON_Mesh* pMesh, int N, ON_3dPointArray* P,
                                     ON_SimpleArray<int>* FI) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  // compute the radius from desired number
  const double r = [&matV, &matF](const int n) {
    Eigen::VectorXd A;
    igl::doublearea(matV, matF, A);
    return sqrt(((A.sum() * 0.5 / (n * 0.6162910373)) / igl::PI));
  }(N);

  MatrixXd matB, matP;
  VectorXi faceI;

  igl::blue_noise(matV, matF, r, matB, faceI, matP);

  cvtEigenToON_Points(matP, P);
  cvtEigenVToON_Array(faceI, FI);
}

void IGM_principal_curvature(ON_Mesh* pMesh, unsigned r, ON_3dPointArray* PD1,
                             ON_3dPointArray* PD2, ON_SimpleArray<double>* PV1,
                             ON_SimpleArray<double>* PV2) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  MatrixXd mPD1, mPD2;
  VectorXd mPV1, mPV2;
  igl::principal_curvature(matV, matF, mPD1, mPD2, mPV1, mPV2, r);

  cvtEigenToON_Points(mPD1, PD1);
  cvtEigenToON_Points(mPD2, PD2);

  cvtEigenVToON_Array(mPV1, PV1);
  cvtEigenVToON_Array(mPV2, PV2);
}

void IGM_gaussian_curvature(ON_Mesh* pMesh, ON_SimpleArray<double>* K) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  VectorXd vecK;
  igl::gaussian_curvature(matV, matF, vecK);
  SparseMatrix<double> M, Minv;
  igl::massmatrix(matV, matF, igl::MASSMATRIX_TYPE_DEFAULT, M);
  igl::invert_diag(M, Minv);

  // Divide by area to get integral average
  auto aveK = (Minv * vecK).eval();

  cvtEigenVToON_Array(aveK, K);
}

void IGM_fast_winding_number(ON_Mesh* pMesh, ON_SimpleArray<double>* Q,
                             ON_SimpleArray<double>* W) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

  MatrixXd matQ;
  cvtArrayToEigenXt(Q->Array(), Q->Count() / 3, matQ);

  VectorXd vecW;
  igl::fast_winding_number(matV, matF, matQ, vecW);

  cvtEigenVToON_Array(vecW, W);
}

void IGM_signed_distance(ON_Mesh* pMesh, ON_SimpleArray<double>* Q, int type,
                         ON_SimpleArray<double>* S, ON_SimpleArray<int>* I,
                         ON_3dPointArray* C) {
  MatrixXd matV;
  MatrixXi matF;
  cvtMeshToEigen(pMesh, matV, matF);

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
  cvtEigenVToON_Array(vecS, S);
  cvtEigenVToON_Array(vecI, I);
  cvtEigenToON_Points(matC, C);
}
