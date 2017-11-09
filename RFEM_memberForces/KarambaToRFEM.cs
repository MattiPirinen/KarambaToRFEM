using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dlubal.RFEM5;
using Karamba;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RFEM_memberForces
{
    class KarambaToRFEM
    {
        public static void SendModel(Karamba.Models.Model kModel)
        {
            Node[] rNodes = Nodes(kModel.nodes);
            NodalSupport[] rSupports = Supports(kModel.supports);
            IApplication app = null;
            IModel rModel = null;
            
            try
            {
                //Get active RFEM5 application
                app = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                app.LockLicense();
                rModel = app.GetActiveModel();
                IModelData rData = rModel.GetModelData();
                object testi = rData.GetCrossSectionDatabase();

                rData.PrepareModification();
                rData.SetNodes(rNodes);
                rData.SetNodalSupports(rSupports);
                rData.FinishModification();
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

        //This method turns karamba nodes into rfem nodes
        public static Node[] Nodes(List<Karamba.Nodes.Node> kNodes)
        {
            List<Node> rNodes = new List<Node>();
            foreach (Karamba.Nodes.Node kNode in kNodes)
            {
                Node RNode = new Node();
                RNode.X = kNode.pos.X;
                RNode.Y = kNode.pos.Z;
                RNode.Z = kNode.pos.Z;
                RNode.No = kNode.ind+1;
                rNodes.Add( RNode);
            }
            return rNodes.ToArray();
        }
        
        //this method turns karamba beams into RFEM members
        public static List<Member> Members(List<Karamba.Elements.ModelBeam> kBeams)
        {
            List<Member> rMembers = new List<Member>();
            foreach (Karamba.Elements.ModelBeam kBeam in kBeams)
            {
                Member RMember = new Member();
                
                rMembers.Add(RMember);


            }
            return rMembers;
        }

        public static NodalSupport[] Supports(List<Karamba.Supports.Support> kSupports)
        {
            Dictionary<List<Boolean>,NodalSupport> supportList = new Dictionary<List<Boolean>, NodalSupport>();



            int i = 1;
            foreach (Karamba.Supports.Support kSupport in kSupports)
            {
                if (!supportList.ContainsKey(kSupport._condition))
                {
                    NodalSupport rSupport = new NodalSupport();
                    rSupport.No = i;
                    SupportConditions(ref rSupport, kSupport);
                    rSupport.NodeList = (kSupport.node_ind+1).ToString();
                    i++;
                    supportList.Add(kSupport._condition,rSupport);
                }
                else
                {
                    NodalSupport rSupport = supportList[kSupport._condition];
                    rSupport.NodeList += "," + (kSupport.node_ind + 1).ToString();
                    supportList[kSupport._condition] = rSupport;
                }
            }
            return supportList.Values.ToArray();
        }

        private static void SupportConditions(ref NodalSupport rSupport, Karamba.Supports.Support kSupport)
        {
            if (kSupport._condition[0]) { rSupport.SupportConstantX = -1; }
            else { rSupport.SupportConstantX = 0; }

            if (kSupport._condition[1]) { rSupport.SupportConstantY = -1; }
            else { rSupport.SupportConstantY = 0; }

            if (kSupport._condition[2]) { rSupport.SupportConstantZ = -1; }
            else { rSupport.SupportConstantZ = 0; }

            if (kSupport._condition[3]) { rSupport.RestraintConstantX = -1; }
            else { rSupport.RestraintConstantX = 0; }

            if (kSupport._condition[4]) { rSupport.RestraintConstantY = -1; }
            else { rSupport.RestraintConstantY = 0; }

            if (kSupport._condition[5]) { rSupport.RestraintConstantZ = -1; }
            else { rSupport.RestraintConstantZ = 0; }

        }


    }
}
