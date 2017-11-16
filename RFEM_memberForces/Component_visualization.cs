using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using System.Drawing;

namespace RFEM_memberForces
{
    public class Component_visualization : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Component_visualization class.
        /// </summary>
        public Component_visualization()
          : base("Component_visualization", "Nickname",
              "Description",
              "RFEM", "misc")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddAngleParameter("a", "a", "a", GH_ParamAccess.item);
            pManager.AddAngleParameter("b", "b", "b", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddArcParameter("u", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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




        private List<RectangleF> _recList = new List<RectangleF>();
        private bool[] _selected = { false, false, false, false,false,false };

        protected override void Layout()
        {
            base.Layout();
            _recList = new List<RectangleF>();
            System.Drawing.Rectangle rec0 = GH_Convert.ToRectangle(Bounds);
            rec0.Height += 22;
            rec0.Width = 110;


            System.Drawing.Rectangle rec1 = rec0;
            rec1.Y = rec1.Bottom - 22;
            rec1.Height = 22;
            rec1.Inflate(-2, -2);
            _width = rec0.Width;
            Bounds = rec0;
            ButtonBounds = rec1;
            for (int i = 1; i < 7; i++)
            {
                _recList.Add(new RectangleF(Bounds.Left + 15 * i, Bounds.Bottom - 15, 10, 10));
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
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }


}