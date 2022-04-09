// ====================================================================
// This file is for testing the cpp functions without Rhino in the loop
// ====================================================================
#include <igl/read_triangle_mesh.h>
#include <spdlog/spdlog.h>

#include <fstream>
#include <iostream>

#include "../igmCppPort/cppFunctions.h"
#include "../igmCppPort/geolib.h"
#include "../igmCppPort/json_helper.h"

int main() {
  MatrixXd matV;
  MatrixXi matF;
  igl::read_triangle_mesh("D:\\testingData\\test.obj", matV, matF);

  string jsonName = "D:\\testingData\\data.json";
  std::ifstream fileIn(jsonName);
  json j = json::parse(fileIn);

  VectorXi conIdx = j["idx"].get<VectorXi>();
  VectorXd conVal = j["val"].get<VectorXd>();

  // solve scalar field
  VectorXd meshScalar(matV.rows());
  GeoLib::solveScalarField(matV, matF, conIdx, conVal, meshScalar);

  // extract isolines
  map<double, MatrixXd> tmpIsoPts;
  VectorXd tmpVal(3);
  tmpVal << 0.3, 0.5, 1.2;
  GeoLib::computeIsoPts(matV, matF, meshScalar, tmpVal, tmpIsoPts);

  int x = 5;

  // testFunc();
}
