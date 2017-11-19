using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dlubal.RFEM5;
using Rhino.Geometry;

namespace RFEM_memberForces.Classes
{

    //This class saves one RFEM support type and all the nodes that link to that support type
    public class SupportInf
    {
        public NodalSupport Support { get; set; }
        public List<Point3d> Points { get; set; }
        public SupportInf(List<Point3d> pointList,NodalSupport support)
        {
            Points = pointList;
            Support = support;
        }
        public SupportInf() { }

        public override string ToString()
        {
            return $"Tx= {Support.SupportConstantX} [N/m], Ty= {Support.SupportConstantY} [N/m], Tz= {Support.SupportConstantZ} [N/m], \r\n" +
                $"Rx= {Support.RestraintConstantX} [N/rad], Ry= {Support.RestraintConstantY} [N/rad], Rz= {Support.RestraintConstantZ}[N/rad].";
        }


    }
}
