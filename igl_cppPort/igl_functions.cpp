
#include <numeric>

#include "igl_functions.h"
#include "igl/adjacency_list.h"
#include "igl/parula.h"

#include "geolib.h"
#include "igl/boundary_loop.h"

using namespace std;
using namespace Eigen;

// helper function
void convertArrayToEigenXd(float* inputArray, int sz, Eigen::MatrixXd& outputEigen)
{
  int cnt = 0;
  outputEigen.resize(sz, 3);

  while (cnt != sz) {
    outputEigen(cnt, 0) = inputArray[cnt * 3];
    outputEigen(cnt, 1) = inputArray[cnt * 3 + 1];
    outputEigen(cnt, 2) = inputArray[cnt * 3 + 2];
    //outputEigen.row(cnt) << inputArray[cnt * 3], inputArray[cnt * 3 + 1], inputArray[cnt * 3 + 2];
    cnt++;
  }
}

void convertArrayToEigenXi(int* inputArray, int sz, Eigen::MatrixXi& outputEigen)
{
  int cnt = 0;
  outputEigen.resize(sz, 3);

  while (cnt != sz) {
    outputEigen.row(cnt) << inputArray[cnt * 3], inputArray[cnt * 3 + 1], inputArray[cnt * 3 + 2];
    cnt++;
  }
}

//RH_C_FUNCTION
double Add(double a, double b) {
  double c = a + b;
  return c;
}

//RH_C_FUNCTION
void igl_adjacency_list(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  convertArrayToEigenXi(F, nF, eigenF);

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

  // the total # of neighbouring vert + the # of vert (as indicator of each vector's size)
  sz = lst.size() + std::accumulate(lst.begin(), lst.end(), (size_t)0, [&](int res, vector<int>& vec) {return res + vec.size(); });
}

void igl_boundary_loop(int* F, int nF, int* adjLst, int& sz) {
  Eigen::MatrixXi eigenF;
  convertArrayToEigenXi(F, nF, eigenF);

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

  // the total # of boundary loops + the # of vert (as indicator of each vector's size)
  sz = lst.size() + std::accumulate(lst.begin(), lst.end(), (size_t)0, [&](int res, vector<int>& vec) {return res + vec.size(); });
}

//RH_C_FUNCTION
void extractIsoLinePts(float* V, int nV, int* F, int nF,
  int* con_idx, double* con_value, int numCon,
  int divN, double* isoLnPts, int* numPtsPerLst)
{
  // size of 'numPtsPerLst'  =  divN;  "numPtsPerLst" contains the # of pts of "isoLnPts' in each isoline

  // convert mesh 
  MatrixXd eigenV;
  MatrixXi eigenF;
  convertArrayToEigenXd(V, nV, eigenV);
  convertArrayToEigenXi(F, nF, eigenF);


  // convert constraints
  VectorXi conIdx(numCon);
  VectorXd conVal(numCon);

  for (size_t i = 0; i < numCon; i++)
  {
    conIdx[i] = *(con_idx + i);
    conVal[i] = *(con_value + i);
  }

  // solve scalar field
  VectorXd meshScalar(eigenV.rows());
  GeoLib::solveScalarField(eigenV, eigenF, conIdx, conVal, meshScalar);

  // extract isolines
  map<double, MatrixXd> tmpIsoPts;
  GeoLib::computeIsoPts(eigenV, eigenF, meshScalar, divN, tmpIsoPts);

  // write data back to the c# pointer arrary
  vector<int> transferNumPtPerLst;
  vector<double> transferIsoLnCollection(0);
  for (auto const& [key, val] : tmpIsoPts) {
    vector<float> transferIsoLn(val.rows() * 3);

    // isoline points
    for (size_t i = 0; i < val.rows(); i++)
    {
      transferIsoLnCollection.emplace_back(val(i, 0));
      transferIsoLnCollection.emplace_back(val(i, 1));
      transferIsoLnCollection.emplace_back(val(i, 2));
    }

    // number of pts per isoline
    transferNumPtPerLst.push_back(val.rows());
  }

  std::copy(transferIsoLnCollection.begin(), transferIsoLnCollection.end(), isoLnPts);
  std::copy(transferNumPtPerLst.begin(), transferNumPtPerLst.end(), numPtsPerLst);
}

//RH_C_FUNCTION
void computeLaplacian(float* V, int nV, int* F, int nF,
  int* con_idx, double* con_value, int numCon, float* laplacianValue)
{
  // size of 'numPtsPerLst'  =  divN;  "numPtsPerLst" contains the # of pts of "isoLnPts' in each isoline

  // convert mesh 
  MatrixXd eigenV;
  MatrixXi eigenF;
  convertArrayToEigenXd(V, nV, eigenV);
  convertArrayToEigenXi(F, nF, eigenF);


  // convert constraints
  VectorXi conIdx(numCon);
  VectorXd conVal(numCon);

  for (size_t i = 0; i < numCon; i++)
  {
    conIdx[i] = *(con_idx + i);
    conVal[i] = *(con_value + i);
  }

  // solve scalar field
  VectorXd meshScalar(eigenV.rows());
  GeoLib::solveScalarField(eigenV, eigenF, conIdx, conVal, meshScalar);


  // transfer data back
  VectorXf meshScalarFloat = meshScalar.cast<float>();
  Eigen::VectorXf::Map(laplacianValue, meshScalarFloat.rows()) = meshScalarFloat;
}

