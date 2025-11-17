using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;

namespace igm {
internal class Helper {
  public static void
  SelectMode(GH_Component _this, object sender, EventArgs e, ref string _mode, string _setTo) {
    _mode = _setTo;
    _this.Message = _mode.ToUpper();
    _this.ExpireSolution(true);
  }
}
}
