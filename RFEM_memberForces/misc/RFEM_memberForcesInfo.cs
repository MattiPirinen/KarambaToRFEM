using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace RFEM_memberForces
{
    public class RFEM_memberForcesInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "RFEMmemberForces";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("30d1ebee-6147-455f-bded-9c182470931b");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Ramboll";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
