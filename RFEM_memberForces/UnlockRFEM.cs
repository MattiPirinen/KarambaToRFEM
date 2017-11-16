using System;
using System.Collections.Generic;
using Dlubal.RFEM5;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RFEM_memberForces
{
    public class UnlockRFEM : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the UnlockRFEM class.
        /// </summary>
        public UnlockRFEM()
          : base("UnlockRFEM", "Nickname",
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
            IApplication app = null;
            try
            {
                //Get active RFEM5 application
                app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                app.UnlockLicense();
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
            get { return new Guid("37565ea2-7485-4016-9a97-23379079185a"); }
        }
    }
}