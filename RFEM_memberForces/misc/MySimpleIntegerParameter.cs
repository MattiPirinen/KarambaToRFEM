using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Drawing;
using Grasshopper.GUI.Canvas;


namespace RFEM_memberForces
{
    public class MySimpleIntegerParameter : GH_Param<Grasshopper.Kernel.Types.GH_Integer>
    {
        public MySimpleIntegerParameter() :
          base(new GH_InstanceDescription("Integer with stats", "Int(stats)",
                                          "Integer with basic statistics",
                                          "Params", "Primitive"))
        { }

        public override System.Guid ComponentGuid
        {
            get { return new Guid("{33D07726-8298-4104-9EBC-5398D8AD5421}"); }
        }

        public override void CreateAttributes()
        {
            m_attributes = new MySimpleIntegerParameterAttributes(this);
        }
        public int MedianValue { get { return 1; }  }
        public int MeanValue { get { return 2; } }


    }


    public class MySimpleIntegerParameterAttributes : GH_Attributes<MySimpleIntegerParameter>
    {
        public MySimpleIntegerParameterAttributes(MySimpleIntegerParameter owner) : base(owner) { }

        protected override void Layout()
        {
            // Compute the width of the NickName of the owner (plus some extra padding), 
            // then make sure we have at least 80 pixels.
            int width = GH_FontServer.StringWidth(Owner.NickName, GH_FontServer.Standard);
            width = Math.Max(width + 10, 80);

            // The height of our object is always 60 pixels
            int height = 60;

            // Assign the width and height to the Bounds property.
            // Also, make sure the Bounds are anchored to the Pivot
            Bounds = new RectangleF(Pivot, new SizeF(width, height));
            
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            // Render all the wires that connect the Owner to all its Sources.
            if (channel == GH_CanvasChannel.Wires)
            {
                RenderIncomingWires(canvas.Painter, Owner.Sources, Owner.WireDisplay);
                return;
            }

            // Render the parameter capsule and any additional text on top of it.
            if (channel == GH_CanvasChannel.Objects)
            {
                // Define the default palette.
                GH_Palette palette = GH_Palette.Normal;

                // Adjust palette based on the Owner's worst case messaging level.
                switch (Owner.RuntimeMessageLevel)
                {
                    case GH_RuntimeMessageLevel.Warning:
                        palette = GH_Palette.Warning;
                        break;

                    case GH_RuntimeMessageLevel.Error:
                        palette = GH_Palette.Error;
                        break;
                }

                // Create a new Capsule without text or icon.
                GH_Capsule capsule = GH_Capsule.CreateCapsule(Bounds, palette);

                // Render the capsule using the current Selection, Locked and Hidden states.
                // Integer parameters are always hidden since they cannot be drawn in the viewport.
                capsule.Render(graphics, Selected, Owner.Locked, true);

                // Always dispose of a GH_Capsule when you're done with it.
                capsule.Dispose();
                capsule = null;

                // Now it's time to draw the text on top of the capsule.
                // First we'll draw the Owner NickName using a standard font and a black brush.
                // We'll also align the NickName in the center of the Bounds.
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Our entire capsule is 60 pixels high, and we'll draw 
                // three lines of text, each 20 pixels high.
                RectangleF textRectangle = Bounds;
                textRectangle.Height = 20;

                // Draw the NickName in a Standard Grasshopper font.
                graphics.DrawString(Owner.NickName, GH_FontServer.Standard, Brushes.Black, textRectangle, format);


                // Now we need to draw the median and mean information.
                // Adjust the formatting and the layout rectangle.
                format.Alignment = StringAlignment.Near;
                textRectangle.Inflate(-5, 0);

                textRectangle.Y += 20;
                graphics.DrawString(String.Format("Median: {0}", Owner.MedianValue),
    
                                    GH_FontServer.StandardItalic, Brushes.Black,
    
                                    textRectangle, format);

                textRectangle.Y += 20;
                graphics.DrawString(String.Format("Mean: {0:0.00}", Owner.MeanValue),
    
                                    GH_FontServer.StandardItalic, Brushes.Black,
    
                                    textRectangle, format);

                // Always dispose of any GDI+ object that implement IDisposable.
                format.Dispose();
            }
        }
    }
          


}