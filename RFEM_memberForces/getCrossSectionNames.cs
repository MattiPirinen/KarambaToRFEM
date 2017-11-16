using System;
using System.Collections.Generic;
using Dlubal.RFEM5;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RFEM_memberForces
{
    public class getCrossSectionNames : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the getCrossSectionNames class.
        /// </summary>
        public getCrossSectionNames()
          : base("getCrossSectionNames", "getCrossSectionNames",
              "Description",
              "RFEM", "misc")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("GO", "GO", "excecute", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool go = false;
            if (!DA.GetData(0, ref go)) { return; }
            if (go)
            {
                IApplication app = null;
                IModel rModel = null;

                try
                {
                    //Get active RFEM5 application
                    app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                    app.LockLicense();
                    rModel = app.GetActiveModel();
                    IModelData rData = rModel.GetModelData();
                    KarambaToRFEM.printCrossSectionIDs(rData);
                }


                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {

                    //Release COM object
                    if (app != null)
                    {
                        app.UnlockLicense();
                        app = null;
                    }

                    //Cleans Garbage collector for releasing all COM interfaces and objects
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }

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
            get { return new Guid("19dba21d-c949-4e5c-b6da-c84e2a6269c0"); }
        }
    }
}