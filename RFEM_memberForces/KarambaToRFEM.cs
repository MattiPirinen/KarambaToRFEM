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

        //This method turns karamba nodes into rfem nodes
        public List<Node> nodes(List<Karamba.Nodes.Node> kNodes)
        {
            List<Node> rNodes = new List<Node>();
            foreach (Karamba.Nodes.Node kNode in kNodes)
            {
                Node RNode = new Node();
                RNode.X = kNode.pos.X;
                RNode.Y = kNode.pos.Z;
                RNode.Z = kNode.pos.Z;
                RNode.No = kNode.ind;
                rNodes.Add( RNode);
            }
            return rNodes;
        }
        
        //this method turns karamba beams into RFEM members
        public List<Member> members(List<Karamba.Elements.ModelBeam> kBeams)
        {
            List<Member> rMembers = new List<Member>();
            foreach (Karamba.Elements.ModelBeam kBeam in kBeams)
            {
                Member RMember = new Member();
                
                rMembers.Add(RMember);


            }
            return rMembers;
        }

        public List<NodalSupport> supports(List<Karamba.Supports.Support> kSupports)
        {
            List<NodalSupport> rSupports = new List<NodalSupport>();
            foreach (Karamba.Supports.Support kSupport in kSupports)
            {
                NodalSupport RSupport = new NodalSupport();
                
                //RSupport.SupportConstantX = kSupport.
                //rMembers.Add(RMember);
                rSupports.Add(RSupport);

            }
            return rSupports;
        }



         


        Node node = new Node();




    }
}
