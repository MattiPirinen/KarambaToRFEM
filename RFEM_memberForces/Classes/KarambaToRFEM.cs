using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dlubal.RFEM5;
using Karamba;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Karamba.CrossSections;
using Rhino;
using Rhino.Geometry;
using Karamba.Models;
using Karamba.Materials;
using Karamba.Loads;

namespace RFEM_memberForces
{
    class KarambaToRFEM
    {

        private static IApplication _iApp;
        private static IModel _rModel;
        private static ICalculation _calc;

        public static IModel RModel
        {
            get
            {
                return _rModel;
            }

            set
            {
                _rModel = value;
            }
        }


        //Help method to see what crossSections there are in the model
        public static void printCrossSectionIDs(IModelData rData)
        {
            CrossSection[] crosecs = rData.GetCrossSections();
            foreach ( CrossSection crosec in crosecs ) { Rhino.RhinoApp.WriteLine(crosec.TextID); }
            
        }

        //Help method to see what materials there are in the model
        public static void printMaterialIDs(IModelData rData)
        {
            Material[] materials = rData.GetMaterials();
            foreach (Material material in materials) { Rhino.RhinoApp.WriteLine(material.TextID); }

        }

        //This method closes the connection to RFEM
        public static void CloseConnection()
        {
            //Release COM object
            if (_iApp != null)
            {
                _iApp.UnlockLicense();
                _iApp = null;
            }
            //Cleans Garbage collector for releasing all COM interfaces and objects
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        //this method excecutes a calculation
        public static void CalcModel()
        {
            if (_rModel != null)
            {
                _calc = _rModel.GetCalculation();
                _calc.Calculate(LoadingType.LoadCaseType, 1);
            }
            
        }

        //Gets result from the model
        public static List<double> getMemberMoments()
        {
            List<double> vy = new List<double>();
            if (_rModel != null && _calc != null)
            {
                IResults results = _calc.GetResultsInFeNodes(LoadingType.LoadCaseType, 1);
                MemberForces[] mf =  results.GetMemberInternalForces(1, ItemAt.AtIndex, true);
                
                foreach (MemberForces m in mf)
                {
                    vy.Add(m.Forces.Y);
                }
                
            }
            return vy;
        }

        //This method opens connection to the RFEM model
        public static void OpenConnection()
        {
            if (_iApp == null)
            { 
                try
                {
                    //Get active RFEM5 application
                    _iApp = Marshal.GetActiveObject("RFEM5.Application") as IApplication;
                    _iApp.LockLicense();
                    RModel = _iApp.GetActiveModel();
                
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Release COM object
                    if (_iApp != null)
                    {
                        _iApp.UnlockLicense();
                        _iApp = null;
                    }
                    //Cleans Garbage collector for releasing all COM interfaces and objects
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
            }
        }

        public static void CreateModel(Karamba.Models.Model kModel)
        {
            if (RModel != null)
            {
                Node[] rNodes = Nodes(kModel.nodes);
                NodalSupport[] rSupports = Supports(kModel.supports);
                Material[] rMaterials = Materials(kModel.materials);
                CrossSection[] rCrossSections = CrossSections(kModel.crosecs, kModel);
                Tuple<Member[], Dlubal.RFEM5.Line[]> vali = Members(kModel.elems);
                Member[] rMembers = vali.Item1;
                Dlubal.RFEM5.Line[] rLines = vali.Item2;
                LoadCase[] lCases = LoadCases(kModel);
                MemberLoad[] rMemberLoads = MemberLoads(kModel.eloads);
                NodalLoad[] rNodalLoads = NodalLoads(kModel.ploads);
                MemberHinge[] rMemberHinges = MemberHinges(kModel.joints);

                //Get active RFEM5 application
                try
                {
                    IModelData rData = RModel.GetModelData();
                    ILoads rLoads = RModel.GetLoads();

                    //Cleans the model and load data data
                    rData.PrepareModification();
                    rData.Clean();
                    rData.FinishModification();

                    rLoads.PrepareModification();
                    rLoads.Clean();
                    rLoads.FinishModification();

                    //Model elements
                    rData.PrepareModification();

                    rData.SetNodes(rNodes);
                    rData.SetNodalSupports(rSupports);
                    rData.SetMaterials(rMaterials);
                    rData.SetCrossSections(rCrossSections);
                    rData.SetMemberHinges(rMemberHinges);
                    rData.SetLines(rLines);
                    rData.SetMembers(rMembers);

                    rData.FinishModification();

                    //Load cases
                    rLoads.PrepareModification();
                    rLoads.SetLoadCases(lCases);
                    rLoads.FinishModification();

                    //Loads
                    setRFEMmemberLoads(rLoads, rMemberLoads);
                    setRFEMnodalLoads(rLoads, rNodalLoads);


                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }



        }

        //Sets all the nodal loads to RFEM
        private static void setRFEMnodalLoads(ILoads rLoads, NodalLoad[] rNodalLoads)
        {
            Dictionary<int, List<NodalLoad>> loads = new Dictionary<int, List<NodalLoad>>();
            foreach (NodalLoad rNodalLoad in rNodalLoads)
            {
                if (!loads.ContainsKey(int.Parse(rNodalLoad.Comment)))
                {
                    List<NodalLoad> nLoad = new List<NodalLoad>();
                    NodalLoad a = rNodalLoad;
                    a.Comment = "";
                    nLoad.Add(a);

                    loads[int.Parse(rNodalLoad.Comment)] = nLoad;
                }
                else
                {
                    NodalLoad a = rNodalLoad;
                    a.Comment = "";
                    loads[int.Parse(rNodalLoad.Comment)].Add(a);
                }
            }
            foreach (int n in loads.Keys)
            {
                ILoadCase lCase = rLoads.GetLoadCase(n, ItemAt.AtNo);
                lCase.PrepareModification();

                lCase.SetNodalLoads(loads[n].ToArray());
                lCase.FinishModification();
            }
        }

        //Sets all the member loads to RFEM (Only uniform distributed load is implemented
        private static void setRFEMmemberLoads(ILoads rLoads, MemberLoad[] rMemberLoads)
        {

            Dictionary<int, List<MemberLoad>> loads = new Dictionary<int, List<MemberLoad>>();
            foreach (MemberLoad rMemberLoad in rMemberLoads)
            {
                if (!loads.ContainsKey(int.Parse(rMemberLoad.Comment)))
                {
                    List<MemberLoad> nLoad = new List<MemberLoad>();
                    MemberLoad a = rMemberLoad;
                    a.Comment = "";
                    nLoad.Add(a);

                    loads[int.Parse(rMemberLoad.Comment)] = nLoad;
                }
                else
                {
                    MemberLoad a = rMemberLoad;
                    a.Comment = "";
                    loads[int.Parse(rMemberLoad.Comment)].Add(a);
                }
            }
            foreach (int n in loads.Keys)
            {
                ILoadCase lCase = rLoads.GetLoadCase(n, ItemAt.AtNo);
                lCase.PrepareModification();

                lCase.SetMemberLoads(loads[n].ToArray());
                lCase.FinishModification();
            }
        }


        //Create all the load cases
        private static LoadCase[] LoadCases(Karamba.Models.Model kModel)
        {

            HashSet<int> lCaseNumbers = new HashSet<int>();
            //Get load cases used in eLoads
            foreach(Karamba.Loads.ElementLoad eLoad in kModel.eloads)
            {
                lCaseNumbers.Add(eLoad.loadcase);
            }
            //get load cases used in ploads
            foreach(Karamba.Loads.PointLoad pLoad in kModel.ploads)
            {
                lCaseNumbers.Add(pLoad.loadcase);
            }

            List<LoadCase> rLoadCases = new List<LoadCase>();
            //Create loadCases
            foreach(int elem in lCaseNumbers)
            {
                LoadCase lCase = new LoadCase();
                lCase.ToSolve = true;
                lCase.SelfWeight = false;
                if (elem == 0)
                {
                    MessageBox.Show("0 cannot be used as loadCase number in RFEM. The number is changed to 999", "Warning", MessageBoxButtons.OK);
                    lCase.Loading.No = 999;
                }
                else lCase.Loading.No = elem;

                rLoadCases.Add(lCase);
            }

            return rLoadCases.ToArray();
        }
        
        //This method creates memberhinges
        private static MemberHinge[] MemberHinges(List<Karamba.CrossSections.CroSec_Joint> kJoints)
        {
            List<MemberHinge> rMemberHinges = new List<MemberHinge>();

            foreach (Karamba.CrossSections.CroSec_Joint kJoint in kJoints)
            {
                

                if (Array.FindIndex(kJoint.props, k => k != null) < 6)
                {
                    MemberHinge rMemberHinge = new MemberHinge();
                    setMemberHinges(ref rMemberHinge, kJoint,0);
                    rMemberHinge.No = 100+(int)kJoint.ind;
                    rMemberHinges.Add(rMemberHinge);
                }
                int i = 6;
                Boolean test = true;
                while (i < 12) { if (kJoint.props[i] != null) test = false; i++; }
                if (!test)
                {
                    MemberHinge rMemberHinge = new MemberHinge();
                    setMemberHinges(ref rMemberHinge, kJoint, 6);
                    rMemberHinge.No = 200+(int)kJoint.ind;
                    rMemberHinges.Add(rMemberHinge);
                }

            }

            return rMemberHinges.ToArray();

        }

        //this method sets memberhinge releases
        private static void setMemberHinges(ref MemberHinge rHinge, CroSec_Joint kJoint, int i)
        {
            if (kJoint.props[i] != null) rHinge.TranslationalConstantX = (double)kJoint.props[i] * 1000; else rHinge.TranslationalConstantX = -1;
            if (kJoint.props[i+1] != null) rHinge.TranslationalConstantY = (double)kJoint.props[i+1] * 1000; else rHinge.TranslationalConstantY = -1;
            if (kJoint.props[i+2] != null) rHinge.TranslationalConstantZ = (double)kJoint.props[i+2] * 1000; else rHinge.TranslationalConstantZ = -1;
            if (kJoint.props[i+3] != null) rHinge.RotationalConstantX = (double)kJoint.props[i+3] * 1000; else rHinge.RotationalConstantX = -1;
            if (kJoint.props[i+4] != null) rHinge.RotationalConstantY = (double)kJoint.props[i+4] * 1000; else rHinge.RotationalConstantY = -1;
            if (kJoint.props[i+5] != null) rHinge.RotationalConstantZ = (double)kJoint.props[i+5] * 1000; else rHinge.RotationalConstantZ = -1;
        }

        //This method creates node loads
        private static NodalLoad[] NodalLoads(List<Karamba.Loads.PointLoad> kPLoads)
        {
            

            Dictionary<Tuple<Vector3d, Vector3d,int>, NodalLoad> rNodalLoads = new Dictionary<Tuple<Vector3d,Vector3d,int>, NodalLoad>();
            foreach (Karamba.Loads.PointLoad kPLoad in kPLoads)
            {
                if (!rNodalLoads.ContainsKey(new Tuple<Vector3d, Vector3d,int>(kPLoad.force, kPLoad.moment,kPLoad.loadcase)))
                {
                    
                    NodalLoad rNodalLoad = new NodalLoad();
                    rNodalLoad.NodeList = kPLoad.node_ind.ToString();
                    NodalLoadComponent component = new NodalLoadComponent();
                    fillComponent(ref component, kPLoad);
                    rNodalLoad.Component = component;
                    rNodalLoad.NodeList = (kPLoad.node_ind+1).ToString();
                    rNodalLoad.No = rNodalLoads.Count + 1;
                    if (kPLoad.loadcase == 0) rNodalLoad.Comment = "999";
                    else rNodalLoad.Comment = kPLoad.loadcase.ToString();
                    rNodalLoads[new Tuple<Vector3d, Vector3d,int>(kPLoad.force, kPLoad.moment,kPLoad.loadcase)] = rNodalLoad;
                }
                else
                {
                    NodalLoad temp = rNodalLoads[new Tuple<Vector3d, Vector3d,int>(kPLoad.force, kPLoad.moment,kPLoad.loadcase)];
                    temp.NodeList += "," + (kPLoad.node_ind + 1).ToString();
                    rNodalLoads[new Tuple<Vector3d, Vector3d, int>(kPLoad.force, kPLoad.moment,kPLoad.loadcase)] = temp;
                }
                
            }
            return rNodalLoads.Values.ToArray();
        }

        //Fill loads to nodal load component
        private static void fillComponent(ref NodalLoadComponent component, PointLoad kPLoad)
        {
            component.Force = new Point3D { X = kPLoad.force.X*1000, Y = kPLoad.force.Y * 1000, Z = kPLoad.force.Z * 1000 };
            component.Moment = new Point3D { X = kPLoad.moment.X * 1000, Y = kPLoad.moment.Y * 1000, Z = kPLoad.moment.Z * 1000 };
        }

        //This method creates member loads
        private static MemberLoad[] MemberLoads(List<Karamba.Loads.ElementLoad> kElementLoads)
        {
            Dictionary<Tuple<Vector3d,int>, MemberLoad> forceList = new Dictionary<Tuple<Vector3d,int>, MemberLoad>();
            foreach (Karamba.Loads.ElementLoad kElementLoad in kElementLoads)
            {
                if (kElementLoad.GetType() == typeof(Karamba.Loads.UniformlyDistLoad))
                    createUniformLoad(kElementLoad, ref forceList);

            }
            return forceList.Values.ToArray();
        }
        //this method creates uniform member load
        private static void createUniformLoad(Karamba.Loads.ElementLoad kElementLoad, ref Dictionary<Tuple<Vector3d,int>, MemberLoad> forceList)
        {

            Karamba.Loads.UniformlyDistLoad load1 = kElementLoad as Karamba.Loads.UniformlyDistLoad;
            if (!forceList.ContainsKey(new Tuple<Vector3d, int>(load1.Load, load1.loadcase)))
            {
                MemberLoad rMemberLoad = new MemberLoad();
                if (load1.Load.X != 0)
                {
                    rMemberLoad.Direction = LoadDirectionType.GlobalXType;
                    rMemberLoad.Magnitude1 = load1.Load.X * 1000;
                }
                else if (load1.Load.Y != 0)
                {
                    rMemberLoad.Direction = LoadDirectionType.GlobalYType;
                    rMemberLoad.Magnitude1 = load1.Load.Y * 1000;
                }
                else
                {
                    rMemberLoad.Direction = LoadDirectionType.GlobalZType;
                    rMemberLoad.Magnitude1 = load1.Load.Z * 1000;
                }

                rMemberLoad.Type = LoadType.ForceType;
                rMemberLoad.Distribution = LoadDistributionType.UniformType;
                if (load1.loadcase == 0)
                {
                    rMemberLoad.Comment = "999";
                }
                else rMemberLoad.Comment = load1.loadcase.ToString();
                int ind;


                if (!string.IsNullOrEmpty(kElementLoad.beamId) && int.TryParse(kElementLoad.beamId.Substring(1), out ind))
                {
                    rMemberLoad.ObjectList = kElementLoad.beamId.Substring(1);
                }
                else
                {
                    MessageBox.Show("Karamba beam naming was incorrect. Has to be of form E001 where E is identifier letter" +
                        "and 001 is number of the beam in the RFEM", "Error", MessageBoxButtons.OK);
                }

                rMemberLoad.No = forceList.Count + 1;
                rMemberLoad.OverTotalLength = true;
                forceList[new Tuple<Vector3d,int>(load1.Load,load1.loadcase)] = rMemberLoad;

            }
            else
            {
                MemberLoad temp = forceList[Tuple.Create(load1.Load,load1.loadcase)];

                int ind;
                if (!string.IsNullOrEmpty(kElementLoad.beamId) && int.TryParse(kElementLoad.beamId.Substring(1), out ind))
                {
                    temp.ObjectList += "," + kElementLoad.beamId.Substring(1);
                }
                else
                {
                    MessageBox.Show("Karamba beam naming was incorrect. Has to be of form E001 where E is identifier letter" +
                        "and 001 is number of the beam in the RFEM", "Error", MessageBoxButtons.OK);
                }
                forceList[Tuple.Create(load1.Load,load1.loadcase)] = temp;
            }


        }

        private static Material[] Materials(List<FemMaterial> kMaterials)
        {
            List<Material> rMaterials = new List<Material>();
            foreach (Karamba.Materials.FemMaterial kMaterial in kMaterials)
            {
                Material rMaterial = new Material();
                rMaterial.No = (int)kMaterial.ind + 1;
                rMaterial.TextID = getRMaterialName(ref rMaterial, kMaterial);
                rMaterials.Add(rMaterial);
            }
            return rMaterials.ToArray();
        }

        private static string getRMaterialName(ref Material rMaterial, FemMaterial kMaterial)
        {
            if (kMaterial.family == "Concrete") { return "Beton " + kMaterial.name; }
            else if (kMaterial.family == "Steel") { return "NameID|Baustahl S " + kMaterial.name.Substring(1)+ "@TypeID|STEEL@NormID|SFS EN 1993-1-1"; }
            //else if (kMaterial.family == "Steel") { return "Baustahl S" + kMaterial.name.Substring(1); }

            else { return "Beton C30/37"; }
        }

        //This method turns karamba nodes into rfem nodes
        private static Node[] Nodes(List<Karamba.Nodes.Node> kNodes)
        {
            List<Node> rNodes = new List<Node>();
            foreach (Karamba.Nodes.Node kNode in kNodes)
            {
                Node RNode = new Node();
                RNode.X = kNode.pos.X;
                RNode.Y = kNode.pos.Y;
                RNode.Z = kNode.pos.Z;
                RNode.No = kNode.ind+1;
                rNodes.Add( RNode);
            }
            return rNodes.ToArray();
        }

        //TODO this method is not ready yet!!!
        //this method turns karamba beams into RFEM members 
        private static Tuple<Member[],Dlubal.RFEM5.Line[]> Members(List<Karamba.Elements.ModelElement> kElems)
        {
            
            List<Member> rMembers = new List<Member>();
            List<Dlubal.RFEM5.Line> rLines = new List<Dlubal.RFEM5.Line>();
            foreach (Karamba.Elements.ModelElement kElem in kElems)
            {
                if (typeof(Karamba.Elements.ModelBeam) == kElem.GetType())
                {

                    Karamba.Elements.ModelBeam kBeam = kElem as Karamba.Elements.ModelBeam;

                    //Creating RFEM line
                    Dlubal.RFEM5.Line rLine = new Dlubal.RFEM5.Line();
                    rLine.No = kBeam.ind + 1;
                    rLine.NodeList = (kBeam._node_inds[0] + 1).ToString() + "," + (kBeam._node_inds[kBeam._node_inds.Count - 1] + 1).ToString();
                    rLines.Add(rLine);

                    //Creating RFEM member
                    Member rMember = new Member();
                    rMember.LineNo = rLine.No;
                    int ind;
                    if (!string.IsNullOrEmpty(kBeam.id) && int.TryParse(kBeam.id.Substring(1), out ind))
                    {
                        rMember.No =ind;
                    }
                    else
                    {
                        rMember.No = kBeam.ind + 1;
                        
                    }


                    rMember.EndCrossSectionNo = (int)kBeam.crossection.ind + 1;
                    rMember.StartCrossSectionNo = rMember.EndCrossSectionNo;

                    //member hinges
                    if (kBeam.joint != null)
                    {
                        //member start
                        if (Array.FindIndex(kBeam.joint.props, k => k != null) < 5) rMember.StartHingeNo = 100 + (int)kBeam.joint.ind;

                        //memberEnd
                        int i = 6;
                        Boolean test = true;
                        while (i < 12) { if (kBeam.joint.props[i] != null) test = false; i++; }
                        if (!test)
                            rMember.EndHingeNo = 200 + (int)kBeam.joint.ind;

                    }

                    rMember.Rotation = new Rotation { Angle = kBeam.res_alpha*2*Math.PI/360, Type=RotationType.Angle};
                    rMembers.Add(rMember);
                }
            }
            return new Tuple<Member[], Dlubal.RFEM5.Line[]>(rMembers.ToArray(), rLines.ToArray());
        }

        //this method turns karamba cross sections into RFEM ones
        private static CrossSection[] CrossSections(List<Karamba.CrossSections.CroSec> kCrossSections,Karamba.Models.Model kModel)
        {
            List<CrossSection> rCrossSections = new List<CrossSection>();

            foreach (Karamba.CrossSections.CroSec kCrossSection in kCrossSections)
            {
                CrossSection rCrossSection = new CrossSection();
                rCrossSection.No = (int)kCrossSection.ind + 1;
                rCrossSection.TextID = getRName(kCrossSection);
                rCrossSection.MaterialNo = getRMaterial(kModel, kCrossSection);//kCrossSection.elemIds[0];
                rCrossSections.Add(rCrossSection);
            }

            return rCrossSections.ToArray();
        }

        //Gets material from the RFEM material library
        private static int getRMaterial(Karamba.Models.Model kModel, CroSec kCrossSection)
        {
            if (kCrossSection.elemIds.Count == 1 && kCrossSection.elemIds[0] == "")
            {
                foreach (Karamba.Materials.FemMaterial material in kModel.materials)
                {
                    if (material.elemIds.Count != 0) { return (int)material.ind + 1; }
                }
                    
                
            } 
            else
            {
                foreach (Karamba.Materials.FemMaterial material in kModel.materials)
                {
                    if (material.elemIds.Count != 0 && material.elemIds.Contains(kCrossSection.elemIds[0]))
                    { return (int)material.ind + 1; }

                }
               
            }
            return 1;
        }

        //Changes the cross-section into RFEM cross section
        private static string getRName(CroSec kCrossSection)
        {
            string kName = kCrossSection.name;
            if (kName.Length >= 4)
            {
                if (kName.Substring(0, 3) == "IPE")
                    return kName.Substring(0, 3) + " " + kName.Substring(3);
                else if (kName.Substring(0, 4) == "RHSC")
                    return "RHS" + " " + kName.Substring(4).Split('.')[0] + " (Ruukki)";
                else if (kName.Substring(0, 4) == "SHSC")
                {
                    string[] size = kName.Substring(4).Split('x');
                    return "SHS" + " " + size[0] + "x" + size[0] + "x" + size[1].Split('.')[0] + " (Ruukki)";
                }
            }
            if (kName == "")
            {
                if (kCrossSection.GetType() == typeof(Karamba.CrossSections.CroSec_Trapezoid))
                {
                    return "SB " + (kCrossSection.dims[4] * 1000).ToString() + "/" + (kCrossSection.dims[2] * 1000).ToString() + "/" +
                        (kCrossSection.dims[0] * 1000).ToString();
                }
                else if (kCrossSection.GetType() == typeof(Karamba.CrossSections.CroSec_I))
                {
                    return "IU " + (kCrossSection.dims[0] * 1000).ToString() + "/" + (kCrossSection.dims[4] * 1000).ToString() + "/" +
                        (kCrossSection.dims[5] * 1000).ToString() + "/" + (kCrossSection.dims[1] * 1000).ToString() + "/" +
                        (kCrossSection.dims[2] * 1000).ToString() + "/" + (kCrossSection.dims[3] * 1000).ToString() + "/" +
                        (kCrossSection.dims[6] * 1000).ToString() + "/" + (kCrossSection.dims[6] * 1000).ToString();
                }

                // Box not working yet.
                /*
                else if (kCrossSection.GetType() == typeof(Karamba.CrossSections.CroSec_Box))
                {
                    return "TR " + (kCrossSection.dims[4] * 1000).ToString() + "/" + (kCrossSection.dims[5] * 1000).ToString() + "/" +
                        (kCrossSection.dims[1] * 1000).ToString() + "/" + (kCrossSection.dims[0] * 1000).ToString() + "/" +
                        (kCrossSection.dims[2] * 1000).ToString() + "/" + (kCrossSection.dims[3] * 1000).ToString();
                }
                */


            }


            return "RECHTECK 100/200"; // Only IPE cross sections are implemented yet.

        }

        //translate karamba nodal supports to rfem nodal supports
        // since in karamba every nodal support is an independent class instance and
        // in rfem only different types of nodal supports need an class instance and
        // the supports are saved as node list inside of the instance. We need to 
        // do some modifications while doing to translation.
        private static NodalSupport[] Supports(List<Karamba.Supports.Support> kSupports)
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

        //Changes karamba support conditions to rfem support conditions
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
