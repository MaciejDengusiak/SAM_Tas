﻿using SAM.Analytical.Systems;
using TPD;

namespace SAM.Analytical.Tas.TPD
{
    public static partial class Convert
    {
        public static SystemDXCoilUnit ToSAM(this DXCoilUnit dXCoilUnit)
        {
            if (dXCoilUnit == null)
            {
                return null;
            }

            dynamic @dynamic = dXCoilUnit as dynamic;

            double coolingDuty = System.Convert.ToDouble((dXCoilUnit.CoolingDuty as dynamic).Value);
            double heatingDuty = System.Convert.ToDouble((dXCoilUnit.HeatingDuty as dynamic).Value);
            double designFlowRate = System.Convert.ToDouble((dXCoilUnit.DesignFlowRate as dynamic).Value);

            double overallEfficiency = dXCoilUnit.OverallEfficiency.Value;

            SystemDXCoilUnit result = new SystemDXCoilUnit(dynamic.Name) 
            {
                CoolingDuty = coolingDuty,
                HeatingDuty = heatingDuty,
                DesignFlowRate = designFlowRate,
                OverallEfficiency = overallEfficiency
            };

            result.Description = dynamic.Description;
            result.SetReference(((ZoneComponent)dXCoilUnit).Reference());

            return result;
        }
    }
}
