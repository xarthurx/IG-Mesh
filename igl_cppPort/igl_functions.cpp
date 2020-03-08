#include <numeric>

#include "igl_functions.h"
#include "igl/adjacency_list.h"

using namespace std;


// helper function
void convertArrayToEigenXd(float* inputArray, int sz, Eigen::MatrixXd& outputEigen)
{
  int cnt = 0;
  outputEigen.resize(sz, 3);

  while (cnt != sz) {
    outputEigen.row(cnt) << inputArray[cnt * 3], inputArray[cnt * 3 + 1], inputArray[cnt * 3 + 2];
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

RH_C_FUNCTION
double Add(double a, double b) {
  double c = a + b;
  return c;
}

RH_C_FUNCTION
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
