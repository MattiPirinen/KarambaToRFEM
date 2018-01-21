using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using System.Drawing;
using Dlubal.RFEM5;
using RFEM_memberForces.Classes;
using Grasshopper.Kernel.Types;

namespace RFEM_memberForces
{
    public class RFEM_Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Component_visualization class.
        /// </summary>
        public RFEM_Support()
          : base("RFEM_Support", "RSupport",
              "Creates a RFEM support where springs can be determined to the support",
              "RFEM", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "Pos", "Point coordinates of the supports", GH_ParamAccess.list);
            pManager.AddVectorParameter("Translation Springs", "Trans", "Translation spring stiffnesses [kN/m]", GH_ParamAccess.item);
            pManager.AddVectorParameter("Rotation Springs", "Rot", "Rotation spring stiffnesses [kN/rad]", GH_ParamAccess.item);
            //TODO Plane not implemented yet!
            //pManager.AddPlaneParameter("Planes", "Plane", "Orientation planes of the supports", GH_ParamAccess.item); 
            pManager[1].Optional = true;
            pManager[2].Optional = true;

            //TODO Plane not implemented yet!
            //pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "Support", "RFEM support", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> ghPoints = new List<Point3d>();
            Vector3d transSprints = new Vector3d(0, 0, 0);
            Vector3d rotSprints = new Vector3d(0, 0, 0);

            //TODO planes Not Implemented
            //Plane locPlane = Plane.WorldXY;


            DA.GetDataList(0,ghPoints);
            DA.GetData(1,ref transSprints);
            DA.GetData(2, ref rotSprints);
            
            //TODO Planes not implemented yet.
            //DA.GetData(3, ref locPlane);

            Attributes_Custom ca = m_attributes as Attributes_Custom;
            NodalSupport support = new NodalSupport();
            if (!ca.SelectedButtons[0]) support.SupportConstantX = transSprints.X * 1000; else support.SupportConstantX = -1;
            if (!ca.SelectedButtons[1]) support.SupportConstantY = transSprints.Y * 1000; else support.SupportConstantY = -1;
            if (!ca.SelectedButtons[2]) support.SupportConstantZ = transSprints.Z * 1000; else support.SupportConstantZ = -1;
            if (!ca.SelectedButtons[3]) support.RestraintConstantX = rotSprints.X * 1000; else support.RestraintConstantX = -1;
            if (!ca.SelectedButtons[4]) support.RestraintConstantY = rotSprints.Y * 1000; else support.RestraintConstantY = -1;
            if (!ca.SelectedButtons[5]) support.RestraintConstantZ = rotSprints.Z * 1000; else support.RestraintConstantZ = -1;
            //support.UserDefinedReferenceSystem


            SupportInf container = new SupportInf(ghPoints, support);
            DA.SetData(0, new GH_ObjectWrapper(container));
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
                return RFEM_memberForces.Properties.Resources.Support;
            }
        }
        
        public override void CreateAttributes()
        {
            //base.CreateAttributes();
            //List<IGH_Attributes> attr = new List<IGH_Attributes>() { new Attributes_Custom(this) };
            //base.Attributes.AppendToAttributeTree(attr);
            m_attributes = new Attributes_Custom(this);
        }
        
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3b1cedc1-62c8-47cb-894c-435d8f05485e"); }
        }
    }



    public class Attributes_Custom : Grasshopper.Kernel.Attributes.GH_ComponentAttributes
    {
        public Attributes_Custom(GH_Component owner) : base(owner) { }

        public bool[] SelectedButtons { get { return _selected; } }


        private List<RectangleF> _recList = new List<RectangleF>();
        private bool[] _selected = { false, false, false, false,false,false };
        private string[] texts = { "Tx", "Ty", "Tz", "Mx", "My", "Mz" };
        protected override void Layout()
        {



            base.Layout();
            _recList = new List<RectangleF>();
            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 40;
            rec0.Width = 140;

            Bounds = rec0;
            for (int i = 0; i < 6; i++)
            {
                _recList.Add(new RectangleF((Bounds.Left+9) + 20 * i, Bounds.Bottom - 15, 10, 10));
            }


            leftBottom = new PointF(rec0.Left, rec0.Bottom);
        }
        private System.Drawing.Rectangle ButtonBounds { get; set; }
       

        private int _width;
        private PointF leftBottom;


        private void DrawRadioButton(Graphics graphics, PointF center, bool testi)
        {
            if (testi)
            {
                graphics.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
            }
            else
            {
                graphics.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
                graphics.FillEllipse(Brushes.White, center.X - 4, center.Y - 4, 8, 8);
            }
        }

        private void DrawString(Graphics graphics, PointF upperLeft, string text)
        {
            graphics.DrawString(text, new Font("Verdena",(float)6.5), Brushes.Black, upperLeft);
        }

        protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);



            if (channel == GH_CanvasChannel.Objects)
            {

                //GH_Capsule button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, GH_Palette.Black, "Button", 2, 0);
                //button.Render(graphics, Selected, Owner.Locked, false);
                //button.Dispose();

                for (int i = 0; i < _selected.Length; i++)
                {
                    DrawRadioButton(graphics, new PointF(_recList[i].Left+5, _recList[i].Bottom - 5), _selected[i]);
                    DrawString(graphics, new PointF(_recList[i].Left - 4, _recList[i].Bottom - 24), texts[i]);

                }
                

            }
        }
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                System.Drawing.RectangleF rec = ButtonBounds;
                for (int i= 0;i<_recList.Count;i++)
                {
                    if (_recList[i].Contains(e.CanvasLocation))
                    {
                        _selected[i]=!_selected[i];
                        Owner.ExpireSolution(true);
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }


}