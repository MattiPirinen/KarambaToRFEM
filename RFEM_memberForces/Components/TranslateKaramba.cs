﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Dlubal.RFEM5;

namespace RFEM_memberForces
{
    public class TranslateKaramba : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TranslateKaramba class.
        /// </summary>
        public TranslateKaramba()
          : base("TranslateKaramba", "TK",
              "translate karamba model data into rfem model data.",
              "RFEM", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Karamba model,", "kModel", "Karamba model", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Excecute", "GO", "Sends the model data to RFEM", GH_ParamAccess.item);
            pManager.AddBooleanParameter("keep Model open", "Open", "If true keeps RFEM connection open", GH_ParamAccess.item);
            pManager[2].Optional = true;
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
            Karamba.Models.GH_Model kModel = null;
            bool kOpen = false;
            if (!DA.GetData(1,ref go)) { return; }
            if (!DA.GetData(0, ref kModel)) { return; }
            DA.GetData(2, ref kOpen);
            if (kOpen){
                KarambaToRFEM.OpenConnection();
                if (go)
                {
                    KarambaToRFEM.CreateModel(kModel.Value);
                }
            }
            else
            {
                if (go)
                {
                    KarambaToRFEM.OpenConnection();
                    KarambaToRFEM.CreateModel(kModel.Value);
                }
                KarambaToRFEM.CloseConnection();
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
            get { return new Guid("986612ec-eb65-447d-9814-af29513b730c"); }
        }
    }
}