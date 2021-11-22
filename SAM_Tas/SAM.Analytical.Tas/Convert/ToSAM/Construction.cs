﻿using System.Collections.Generic;

namespace SAM.Analytical.Tas
{
    public static partial class Convert
    {
        public static Construction ToSAM(this TBD.Construction construction)
        {
            if(construction == null)
            {
                return null;
            }

            List<TBD.material> materials = construction.Materials();

            List<ConstructionLayer> constructionLayers = null;
            if(materials != null)
            {
                constructionLayers = new List<ConstructionLayer>();
                for(int i=0; i < materials.Count; i++)
                {
                    TBD.material material = materials[i];
                    double thickness = construction.materialWidth[i];

                    constructionLayers.Add(new ConstructionLayer(material.name, thickness));
                }
            }

            Construction result = new Construction(construction.name, constructionLayers);
            return result;
        }
    }
}
