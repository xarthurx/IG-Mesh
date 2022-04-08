//////////////////////////////////////////////
// @file: json_helper.hpp
// @brief: json helpers for processing data
//
// @author: Zhao Ma
//////////////////////////////////////////////
#pragma once

#include <nlohmann/json.hpp>
#include <Eigen/Core>

using namespace Eigen;
using nlohmann::json;

namespace Eigen {

// ! To Json
inline void to_json(nlohmann::json& j, const VectorXd& v) {
  j = nlohmann::json::array();
  for (int i = 0; i < v.size(); ++i) {
    j.push_back(v[i]);
  }
}

inline void to_json(nlohmann::json& j, const std::vector<Vector3d>& v) {
  j = nlohmann::json::array();
  for (int i = 0; i < v.size(); ++i) {
    j.push_back({v[i][0], v[i][1], v[i][2]});
  }
}

inline void to_json(nlohmann::json& j, const MatrixXd& v) {
  j = nlohmann::json::array();
  if (v.cols() == 3) {
    for (int i = 0; i < v.rows(); ++i) {
      j.push_back({v(i, 0), v(i, 1), v(i, 2)});
    }
  } else if (v.cols() == 2) {
    for (int i = 0; i < v.rows(); ++i) {
      j.push_back({v(i, 0), v(i, 1)});
    }
  }
}

inline void to_json(nlohmann::json& j, const MatrixXi& e) {
  j = nlohmann::json::array();
  if (e.cols() == 3) {
    for (int i = 0; i < e.rows(); ++i) {
      j.push_back({e(i, 0), e(i, 1), e(i, 2)});
    }
  } else if (e.cols() == 2) {
    for (int i = 0; i < e.rows(); ++i) {
      j.push_back({e(i, 0), e(i, 1)});
    }
  }
}

inline void to_json(nlohmann::json& j,
                    const std::vector<std::vector<Vector3d>>& isoLinePts) {
  j = nlohmann::json::array();
  // push all isoline into the strucutre
  for_each(isoLinePts.begin(), isoLinePts.end(), [&](auto& curLn) {
    j.push_back(nlohmann::json::array());

    // push pts if the current isoline into the structure
    for_each(curLn.begin(), curLn.end(), [&](auto& curPt) {
      j.back().push_back({curPt[0], curPt[1], curPt[2]});
    });
  });
}

inline void to_json(nlohmann::json& j, const std::vector<MatrixXd>& isoLinePts) {
  j = nlohmann::json::array();
  // push all isoline into the strucutre
  for_each(isoLinePts.begin(), isoLinePts.end(), [&](auto& curLn) {
    j.push_back(nlohmann::json::array());

    // push pts if the current isoline into the structure
    for (size_t i = 0; i < curLn.rows(); i++) {
      j.back().push_back({curLn(i, 0), curLn(i, 1), curLn(i, 2)});
    }
  });
}

inline void to_json(nlohmann::json& j,
                    const std::tuple<VectorXd, MatrixXd>& vecMap) {
  j = nlohmann::json::array();

  auto [vec, mat] = vecMap;
  for (size_t i = 0; i < mat.rows(); i++) {
    j.push_back({vec[i], mat(i, 0), mat(i, 1), mat(i, 2)});
  }
}

// ! From Json
// Eigen Vectors
inline void from_json(const nlohmann::json& j, VectorXd& v) {
  v.resize(j.size());
  for (int i = 0; i < j.size(); ++i) {
    v[i] = j[i];
  }
}

inline void from_json(const nlohmann::json& j, VectorXi& v) {
  v.resize(j.size());
  for (int i = 0; i < j.size(); ++i) {
    v[i] = j[i];
  }
}

inline void from_json(const nlohmann::json& j, Vector2d& v) {
  for (int i = 0; i < j.size(); ++i) {
    v[i] = j[i];
  }
}

inline void from_json(const nlohmann::json& j, Vector2i& v) {
  for (int i = 0; i < j.size(); ++i) {
    v[i] = j[i];
  }
}

// STL container
inline void from_json(const nlohmann::json& j, std::vector<std::vector<Vector2d>>& v) {
  for (size_t i = 0; i < j.size();
       i++) {  // each item has all the points of a pattern
    std::vector<Vector2d> tmp{};
    for (size_t m = 0; m < j[i].size(); m++) {
      Vector2d v2d = j[i][m].get<Vector2d>();
      tmp.emplace_back(v2d);
    }
    v.emplace_back(tmp);
  }
}

inline void from_json(const nlohmann::json& j, std::vector<std::vector<Vector2i>>& v) {
  for (size_t i = 0; i < j.size();
       i++) {  // each item has all the points of a pattern
    std::vector<Vector2i> tmp{};
    for (size_t m = 0; m < j[i].size(); m++) {
      Vector2i v2i = j[i][m].get<Vector2i>();
      tmp.emplace_back(v2i);
    }
    v.emplace_back(tmp);
  }
}

inline void from_json(const nlohmann::json& j, std::vector<std::vector<double>>& v) {
  for (size_t i = 0; i < j.size();
       i++) {  // each item has all the points of a pattern
    // vector<double> tmp{};
    auto tmp = j[i].get<std::vector<double>>();
    // for (size_t m = 0; m < j[i].size(); m++) {
    //  double val = j[i][m];
    //  tmp.emplace_back(val);
    //}
    v.emplace_back(tmp);
  }
}

// dynamic matrices, for mesh, only 3 column
inline void from_json(const nlohmann::json& j, MatrixX3i& m) {
  m.resize(j.size(), 3);
  for_each(j.begin(), j.end(), [&](auto& row) {
    auto curId = &row - &j[0];
    m.row(curId) << row[0], row[1], row[2];
  });
}

inline void from_json(const nlohmann::json& j, MatrixX3d& m) {
  m.resize(j.size(), 3);
  for_each(j.begin(), j.end(), [&](auto& row) {
    auto curId = &row - &j[0];
    m.row(curId) << row[0], row[1], row[2];
  });
}

inline void from_json(const nlohmann::json& j, MatrixX2d& m) {
  m.resize(j.size(), 2);
  for_each(j.begin(), j.end(), [&](auto& row) {
    auto curId = &row - &j[0];
    m.row(curId) << row[0], row[1];
  });
}

inline void from_json(const nlohmann::json& j, MatrixX2i& m) {
  m.resize(j.size(), 2);
  for_each(j.begin(), j.end(), [&](auto& row) {
    auto curId = &row - &j[0];
    m.row(curId) << row[0], row[1];
  });
}
}  // namespace Eigen