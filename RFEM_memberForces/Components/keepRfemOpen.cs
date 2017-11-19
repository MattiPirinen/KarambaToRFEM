using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GH_IO;
using Rhino.Geometry;
using Dlubal.RFEM5;
using Karamba;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Karamba.CrossSections;
using Rhino;
using Karamba.Models;
using Karamba.Materials;


namespace RFEM_memberForces
{
    public class keepRfemOpen : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the keepRfemOpen class.
        /// </summary>
        public keepRfemOpen()
          : base("keepRfemOpen", "Nickname",
              "Description",
              "RFEM", "misc")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("GO", "GO", "GO", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("model", "model", "model", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool go = false;
            if (!DA.GetData(0, ref go)) { return; }

            IApplication app = null;
            IModel rModel = null;

            if (go)
            {


                try
                {
                    //Get active RFEM5 application
                    app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                    app.LockLicense();
                    rModel = app.GetActiveModel();
                    DA.SetData(0, new GH_ObjectWrapper(rModel));


                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }


            }
            else
            {
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
            get { return new Guid("0e7520d2-c8cb-46bf-ab5d-f0755bc265f1"); }
        }
    }
}