using System;
using System.Collections.Generic;
using Dlubal.RFEM5;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Grasshopper;
using System.Windows.Forms;
using System.Linq;
using Karamba;

namespace RFEM_memberForces
{
    public class RFEMmemberForcesComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RFEMmemberForcesComponent()
          : base("RFEM_memberForces", "Nickname",
              "Get_Member_Forces",
              "RFEM", "GetResults")
        {
        }
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Load case number", "LCase", "The number of the loadCase, from which the results are obtained.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Member numbers", "MNumbers", "Members that results are gathered from." + 
                "If omitted, all the memberResults are gathered.", GH_ParamAccess.list,new List<int>(new int[] { -1 }));
            pManager.AddBooleanParameter("Excecute", "GO", "Gets the results from RFEM", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "Line", "Line", GH_ParamAccess.list);
            pManager.AddNumberParameter("Location", "Location", "Location", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Normal Force", "N", "Normal Force", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Shear Force Vy", "Vy", "Shear Force Vy", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Shear Force Vz", "Vz", "Shear Force Vz", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Moment Mx", "Mx", "Moment Mx", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Moment My", "My", "Moment My", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Moment Mz", "Mz", "Normal Force", GH_ParamAccess.tree);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare variables where input data are saved
            int lCase = 0;
            List<int> mNumbers = new List<int>();
            Boolean go = false;

            if (!DA.GetData(0,ref lCase)) { return; }
            if (!DA.GetDataList(1, mNumbers)) { return; }
            if (!DA.GetData(2, ref go)) { return; }
            if (go)
            {
                Tuple<DataTree<float>[], List<Rhino.Geometry.Line>> data;
                if (mNumbers.Count == 1 && mNumbers[0] == -1) data = runRFEMapp(lCase);
                else data = runRFEMapp(lCase, mNumbers.ToArray());

                DA.SetDataList(0, data.Item2);
                for (int i = 0; i < data.Item1.Length; i++)
                {
                    DA.SetDataTree(i+1, data.Item1[i]);
                }
                
            }



        }

        private Tuple<DataTree<float>[],List<Rhino.Geometry.Line>> runRFEMapp(int LCase,int[] mNumbers)
        {
            IApplication app = null;
            IModel model = null;
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            DataTree<float> location = new DataTree<float>();
            DataTree<float> Nx = new DataTree<float>();
            DataTree<float> Vy = new DataTree<float>();
            DataTree<float> Vz = new DataTree<float>();
            DataTree<float> Mx = new DataTree<float>();
            DataTree<float> My = new DataTree<float>();
            DataTree<float> Mz = new DataTree<float>();
            ItemAt itemAt = ItemAt.AtNo;

            try
            {
                //Get active RFEM5 application
                app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                app.LockLicense();
                model = app.GetActiveModel();

                IModelData data = model.GetModelData();
                ICalculation calc = model.GetCalculation();
                IResults res = calc.GetResultsInFeNodes(LoadingType.LoadCaseType, LCase);
                foreach (int number in mNumbers)
                {
                    try
                    {
                        MemberForces[] forces2 = res.GetMembersInternalForces(true);
                        MemberForces[] forces = res.GetMemberInternalForces(number, itemAt, true);
                        lines.Add(getMemberLine(data,number, itemAt));
                        
                        
                        GH_Path path = new GH_Path(number);

                        for (int i = 0; i <forces.Length;i++ )
                        {
                            location.Insert((float)forces[i].Location, path, i);
                            Nx.Insert((float)forces[i].Forces.X, path, i);
                            Vy.Insert((float)forces[i].Forces.Y, path, i);
                            Vz.Insert((float)forces[i].Forces.Z, path, i);
                            Mx.Insert((float)forces[i].Moments.X, path, i);
                            My.Insert((float)forces[i].Moments.Y, path, i);
                            Mz.Insert((float)forces[i].Moments.Z, path, i);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Couldnt find member no. {number}", ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }


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
            DataTree<float>[] values = new DataTree<float>[] {location, Nx, Vy, Vz, Mx, My, Mz };
            return new Tuple<DataTree<float>[],List<Rhino.Geometry.Line>>(values,lines);
        }

        private Tuple<DataTree<float>[], List<Rhino.Geometry.Line>> runRFEMapp(int LCase)
        {
            IApplication app = null;
            IModel model = null;
            List<Rhino.Geometry.Line> lines = new List<Rhino.Geometry.Line>();
            DataTree<float> location = new DataTree<float>();
            DataTree<float> Nx = new DataTree<float>();
            DataTree<float> Vy = new DataTree<float>();
            DataTree<float> Vz = new DataTree<float>();
            DataTree<float> Mx = new DataTree<float>();
            DataTree<float> My = new DataTree<float>();
            DataTree<float> Mz = new DataTree<float>();
            ItemAt itemAt = ItemAt.AtNo;
            
            try
            {
                //Get active RFEM5 application
                app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                app.LockLicense();
                model = app.GetActiveModel();

                IModelData data = model.GetModelData();
                ICalculation calc = model.GetCalculation();
                IResults res = calc.GetResultsInFeNodes(LoadingType.LoadCaseType, LCase);
                MemberForces[] forces = res.GetMembersInternalForces(true);

                try
                {
                    int i = 0;
                    int k = 0;
                    GH_Path path = new GH_Path(forces[0].MemberNo);
                    lines.Add(getMemberLine(data, forces[0].MemberNo, itemAt));
                    while (i < forces.Length)
                        {
                        if (i > 0 && forces[i].MemberNo != forces[i - 1].MemberNo)
                        {
                            path = new GH_Path(forces[i].MemberNo);
                            lines.Add(getMemberLine(data, forces[i].MemberNo, itemAt));
                            k = 0;
                        }
                        location.Insert((float)forces[i].Location, path, k);
                        Nx.Insert((float)forces[i].Forces.X, path, k);
                        Vy.Insert((float)forces[i].Forces.Y, path, k);
                        Vz.Insert((float)forces[i].Forces.Z, path, k);
                        Mx.Insert((float)forces[i].Moments.X, path, k);
                        My.Insert((float)forces[i].Moments.Y, path, k);
                        Mz.Insert((float)forces[i].Moments.Z, path, k);
                        i += 1;
                        k += 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            DataTree<float>[] values = new DataTree<float>[] { location, Nx, Vy, Vz, Mx, My, Mz };
            return new Tuple<DataTree<float>[], List<Rhino.Geometry.Line>>(values, lines);
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
            get { return new Guid("110b1bd2-ab8a-4e84-b881-fdf0d225618a"); }
        }

        //This method creates a rhino line from the start and end point of the underlying line of the 
        //rfem member.
        private Rhino.Geometry.Line getMemberLine(IModelData data, int number,ItemAt itemAt)
        {
            Member test =  data.GetMember(number, itemAt).GetData();
            


            int lineNo = data.GetMember(number, itemAt).GetData().LineNo;
            Dlubal.RFEM5.Line line = data.GetLine(lineNo, itemAt).GetData();
            
            int[] nodes = Array.ConvertAll(line.NodeList.Split(','), s => int.Parse(s));
            nodes = new int[] { nodes[0], nodes[nodes.Length - 1] };
            Node nodeStart = data.GetNode(nodes[0], itemAt).GetData();
            Node nodeEnd = data.GetNode(nodes[1], itemAt).GetData();
            return new Rhino.Geometry.Line(new Point3d(nodeStart.X, nodeStart.Y, nodeStart.Z),
                            new Point3d(nodeEnd.X, nodeEnd.Y, nodeEnd.Z));
        }


    }



}
