using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace RFEM_memberForces.Components
{
    public class RunModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RunModel class.
        /// </summary>
        public RunModel()
          : base("Run RFEM Model", "RunModel",
              "executes calculation in RFEM",
              "RFEM", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Execute", "GO", "Executes calculation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("testi", "testi", "testi", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Boolean go = false;
            if(!DA.GetData(0,ref go)) { return; }
            if (go)
            {
                KarambaToRFEM.OpenConnection();
                KarambaToRFEM.CalcModel();
                List<double> result = KarambaToRFEM.getMemberMoments();
                KarambaToRFEM.CloseConnection();
                DA.SetDataList(0, result);
            }
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
            get { return new Guid("d1d00a92-7c45-41a8-8ebd-3d375f9655a7"); }
        }
    }
}