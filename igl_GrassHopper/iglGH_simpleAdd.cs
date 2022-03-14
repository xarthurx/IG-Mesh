using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using IGLRhinoCommon;

namespace ghIGL
{
    public class iglGH_simpleAdd: GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public iglGH_simpleAdd()
          : base("Sum", "addSum",
              "example function for using cpp code in c#",
              "IGL+", "testCase")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.

            pManager.AddNumberParameter("Avalue", "A", "first number to add", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Bvalue", "B", "second number to add", GH_ParamAccess.item, 0.0);

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            //pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sum", "S", "sum of a and b", GH_ParamAccess.item);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            var x = 0.0;
            var y = 0.0;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref x)) return;
            if (!DA.GetData(1, ref y)) return;

            //// We should now validate the data and warn the user if invalid data is supplied.
            //if (radius0 < 0.0)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Inner radius must be bigger than or equal to zero");
            //    return;
            //}
            //if (radius1 <= radius0)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Outer radius must be bigger than the inner radius");
            //    return;
            //}
            //if (turns <= 0)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Spiral turn count must be bigger than or equal to one");
            //    return;
            //}

            // We're set to create the spiral now. To keep the size of the SolveInstance() method small, 
            // The actual functionality will be in a different method:
            //Curve spiral = CreateSpiral(plane, radius0, radius1, turns);
            var res = IGLRhinoCommon.Utils.Sum(x, y);

            // Finally assign the spiral to the output parameter.
            DA.SetData(0, res);
        }


        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("dde873b9-32b1-45de-a389-f7df10b18235"); }
        }
    }
}
