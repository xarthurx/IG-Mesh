using Grasshopper.Kernel;
using System;

namespace igmGH
{
    public class IGM_vert_face_normals : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public IGM_vert_face_normals()
          : base("Face Normal", "iNormals_F",
              "Compute the per face normals of the given mesh.",
              "IG-Mesh", "02 | Properties")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input mesh for analysis.", GH_ParamAccess.item);
            //pManager.AddPointParameter("Mesh V", "V", "A list of mesh vertices.", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Mesh F", "F", "A list of mesh faces.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Face Normals", "FN", "the per-face normals of the input mesh's faces", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();
            if (!DA.GetData(0, ref mesh)) { return; }
            if (!mesh.IsValid) { return; }

            // call the cpp function to solve the adjacency list
            var fn = IGMRhinoCommon.Utils.getNormalsFace(ref mesh);

            // output
            DA.SetDataList(0, fn);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3b03e8b2-ffd3-4679-b94e-3582f485a877"); }
        }
    }
}
