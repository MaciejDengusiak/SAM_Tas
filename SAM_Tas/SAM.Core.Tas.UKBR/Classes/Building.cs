﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SAM.Core.Tas.UKBR
{
    public class Building : UKBRElement
    {
        public override string UKBRName => "Building";

        public Building(XElement xElement)
            : base(xElement)
        {

        }

        public LightSetups LightSetups
        {
            get
            {
                return new LightSetups(xElement);
            }
        }

        public SourceSets SourceSets
        {
            get
            {
                return new SourceSets(xElement);
            }
        }

        public Zone GetZone(global::System.Guid guid, global::System.Guid? sourceSetGuid = null)
        {
            SourceSets sourceSets = SourceSets;
            if(sourceSets == null)
            {
                return null;
            }

            foreach(SourceSet sourceSet in sourceSets)
            {
                if (sourceSetGuid != null && sourceSetGuid.HasValue)
                {
                    if (sourceSet.GUID != sourceSetGuid)
                    {
                        continue;
                    }
                }

                Zones zones = sourceSet?.Zones;
                if(zones == null)
                {
                    continue; 
                }

                foreach(Zone zone in zones)
                {
                    if(zone.GUID == guid)
                    {
                        return zone;
                    }
                }
            }

            return null;
        }

        public List<Zone> GetZones(IEnumerable<global::System.Guid> guids, global::System.Guid? sourceSetGuid = null)
        {
            if(guids == null)
            {
                return null;
            }

            SourceSets sourceSets = SourceSets;
            if (sourceSets == null)
            {
                return null;
            }

            List<Zone> result = new List<Zone>();
            foreach (SourceSet sourceSet in sourceSets)
            {
                if(sourceSetGuid != null && sourceSetGuid.HasValue)
                {
                    if(sourceSet.GUID != sourceSetGuid)
                    {
                        continue;
                    }
                }

                Zones zones = sourceSet?.Zones;
                if (zones == null)
                {
                    continue;
                }

                foreach (Zone zone in zones)
                {
                    if (guids.Contains( zone.GUID))
                    {
                        result.Add(zone);
                    }
                }
            }

            return result;
        }

        public List<Zone> GetZones(ZoneGUIDs zoneGUIDs, global::System.Guid? sourceSetGuid = null)
        {
            List<global::System.Guid> guids = zoneGUIDs?.ToList().ConvertAll(x => x.GUID);
            if(guids == null)
            {
                return null;
            }

            return GetZones(guids, sourceSetGuid);
        }

        public List<BuildingElement> GetBuildingElements(IEnumerable<global::System.Guid> guids, global::System.Guid? sourceSetGuid = null)
        {
            if (guids == null)
            {
                return null;
            }

            SourceSets sourceSets = SourceSets;
            if (sourceSets == null)
            {
                return null;
            }

            List<BuildingElement> result = new List<BuildingElement>();
            foreach (SourceSet sourceSet in sourceSets)
            {
                if (sourceSetGuid != null && sourceSetGuid.HasValue)
                {
                    if (sourceSet.GUID != sourceSetGuid)
                    {
                        continue;
                    }
                }

                BuildingElements buildingElements = sourceSet?.BuildingElements;
                if (buildingElements == null)
                {
                    continue;
                }

                foreach (BuildingElement buildingElement in buildingElements)
                {
                    if (guids.Contains(buildingElement.GUID))
                    {
                        result.Add(buildingElement);
                    }
                }
            }

            return result;
        }

        public string NCMVersion
        {
            get
            {
                return Query.Value<string>(xElement?.Attribute("NCMVersion"));
            }
        }

        public double AirPermeability
        {
            get
            {
                return Query.Value(xElement?.Attribute("AirPermeability"), Query.Invalid<double>());
            }
        }

        public int CIBSEBuildingTypeIndex
        {
            get
            {
                return Query.Value(xElement?.Attribute("CIBSEBuildingType"), Query.Invalid<int>());
            }
        }

        public int CountryIndex
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("Country"));
            }
        }

        public int CIBSEBuildingSizeIndex
        {
            get
            {
                return Query.Value(xElement?.Attribute("CIBSEBuildingSize"), Query.Invalid<int>());
            }
        }

        public bool ModelNatVentVia5ACH
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("ModelNatVentVia5ACH"));
            }
        }

        public bool DoBuildingRegsCalcs
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("DoBuildingRegsCalcs"));
            }
        }

        public bool DoEPCCalcs
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("DoEPCCalcs"));
            }
        }

        public double AncillaryFanLoad
        {
            get
            {
                return Query.Value<double>(xElement?.Attribute("AncillaryFanLoad"));
            }
        }

        public double AncillaryPumpLoad
        {
            get
            {
                return Query.Value<double>(xElement?.Attribute("AncillaryPumpLoad"));
            }
        }

        public double TotalVolume
        {
            get
            {
                return Query.Value<double>(xElement?.Attribute("TotalVolume"));
            }
        }

        public int WeatherFileIndex
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("WeatherFile"));
            }
        }

        public bool HVACMonitoring
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("HVACMonitoring"));
            }
        }

        public bool ShowCrit3
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("ShowCrit3"));
            }
        }

        public int TPDxSourceIndex
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("TPDxSource"));
            }
        }

        public bool bSavePostImport
        {
            get
            {
                return Query.Value<bool>(xElement?.Attribute("bSavePostImport"));
            }
        }

        public double FoundationArea
        {
            get
            {
                return Query.Value<double>(xElement?.Attribute("FoundationArea"));
            }
        }

        public int NumberOfStoreys
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("NumberOfStoreys"));
            }
        }

        public int FoundationAreaCalculationMethodIndex
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("FoundationAreaCalculationMethod"));
            }
        }

        public int LightSetupSource
        {
            get
            {
                return Query.Value<int>(xElement?.Attribute("LightSetupSource"));
            }
        }
    }
}
