﻿using SAM.Core.Tas;
using System;
using System.Collections.Generic;
using System.Reflection;
using TSD;

namespace SAM.Analytical.Tas
{
    public static partial class Modify
    {
        public static List<Core.Result> AddResults(this string path_TSD, AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null || string.IsNullOrWhiteSpace(path_TSD))
                return null;

            List<Core.Result> result = null;
            using (SAMTSDDocument sAMTSDDocument = new SAMTSDDocument(path_TSD, true))
            {
                result = AddResults(sAMTSDDocument, adjacencyCluster);
            }

            return result;
        }

        public static List<Core.Result> AddResults(this SAMTSDDocument sAMTSDDocument, AdjacencyCluster adjacencyCluster)
        {
            if (sAMTSDDocument == null || adjacencyCluster == null)
                return null;

            return AddResults(sAMTSDDocument.TSDDocument, adjacencyCluster);
        }

        public static List<Core.Result> AddResults(this TSDDocument tSDDocument, AdjacencyCluster adjacencyCluster)
        {
            if (tSDDocument == null || adjacencyCluster == null)
                return null;

            return AddResults(tSDDocument.SimulationData, adjacencyCluster);
        }

        public static List<Core.Result> AddResults(this SimulationData simulationData, AdjacencyCluster adjacencyCluster)
        {
            if (simulationData == null || adjacencyCluster == null)
                return null;

            List<Core.Result> result = null; 

            //get simulaton data from Tas for individal SAM Space
            List<Core.Result> results = Convert.ToSAM_Results(simulationData);
            if (results == null)
                return result;

            result = new List<Core.Result>(results);

            Dictionary<Guid, List<SpaceSimulationResult>> dictionary = new Dictionary<Guid, List<SpaceSimulationResult>>();
            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces != null && spaces.Count > 0)
            {
                adjacencyCluster.GetPanels()?.ForEach(x => adjacencyCluster.GetRelatedObjects<SurfaceSimulationResult>(x.Guid)?.ForEach(y => adjacencyCluster.RemoveObject<SurfaceSimulationResult>(y.Guid)));

                foreach (Space space in spaces)
                {
                    List<SpaceSimulationResult> spaceSimulationResults_Space = results.FindAll(x => x is SpaceSimulationResult && space.Name.Equals(x.Name)).ConvertAll(x => (SpaceSimulationResult)x);
                    dictionary[space.Guid] = spaceSimulationResults_Space;
                    if(spaceSimulationResults_Space != null && spaceSimulationResults_Space.Count != 0)
                    {
                        foreach (SpaceSimulationResult spaceSimulationResult in spaceSimulationResults_Space)
                        {
                            List<SpaceSimulationResult> spaceSimulationResults_Existing = adjacencyCluster.GetResults<SpaceSimulationResult>(space, Query.Source())?.FindAll(x => x.LoadType() == spaceSimulationResult.LoadType());
                            if (spaceSimulationResults_Existing != null && spaceSimulationResults_Existing.Count != 0)
                            {
                                adjacencyCluster.Remove(spaceSimulationResults_Existing);
                                if (spaceSimulationResults_Existing[0].TryGetValue(Analytical.SpaceSimulationResultParameter.DesignLoad, out double designLoad))
                                {
                                    spaceSimulationResult.SetValue(Analytical.SpaceSimulationResultParameter.DesignLoad, designLoad);
                                }
                            }

                            adjacencyCluster.AddObject(spaceSimulationResult);
                            adjacencyCluster.AddRelation(space, spaceSimulationResult);
                        }
                    }

                    if(!space.TryGetValue(SpaceParameter.ZoneGuid, out string zoneGuid) || zoneGuid == null)
                    {
                        continue;
                    }

                    foreach(Core.Result result_Temp in results)
                    {
                        SurfaceSimulationResult surfaceSimulationResult = result_Temp as SurfaceSimulationResult;
                        if(surfaceSimulationResult == null)
                        {
                            continue;
                        }

                        if(!surfaceSimulationResult.TryGetValue(SurfaceSimulationResultParameter.ZoneSurfaceReference, out ZoneSurfaceReference zoneSurfaceReference) || zoneSurfaceReference == null || !zoneGuid.Equals(zoneSurfaceReference.ZoneGuid))
                        {
                            continue;
                        }

                        adjacencyCluster.AddObject(surfaceSimulationResult);

                        List<Panel> panels = adjacencyCluster.GetPanels(space);
                        if(panels != null && panels.Count != 0)
                        {
                            foreach(Panel panel in panels)
                            {
                                if(panel == null)
                                {
                                    continue;
                                }

                                List<Aperture> apertures = panel.Apertures;
                                if (apertures != null && apertures.Count != 0)
                                {
                                    Aperture aperture = Query.Match(zoneSurfaceReference, apertures, out AperturePart aperturePart);
                                    if(aperture != null)
                                    {
                                        adjacencyCluster.AddRelation(panel, surfaceSimulationResult);
                                        break;
                                    }
                                }

                                if (panel.TryGetValue(PanelParameter.ZoneSurfaceReference_1, out ZoneSurfaceReference zoneSurfaceReference_1) && zoneSurfaceReference_1 != null)
                                {
                                    if(zoneSurfaceReference_1.SurfaceNumber == zoneSurfaceReference.SurfaceNumber)
                                    {
                                        adjacencyCluster.AddRelation(panel, surfaceSimulationResult);
                                        break;
                                    }
                                }

                                if (panel.TryGetValue(PanelParameter.ZoneSurfaceReference_2, out ZoneSurfaceReference zoneSurfaceReference_2) && zoneSurfaceReference_2 != null)
                                {
                                    if (zoneSurfaceReference_2.SurfaceNumber == zoneSurfaceReference.SurfaceNumber)
                                    {
                                        adjacencyCluster.AddRelation(panel, surfaceSimulationResult);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // get data data about SAM Zones like space GUID, Zone Category etc.
            List<Zone> zones = adjacencyCluster.GetZones();
            if (zones != null && zones.Count > 0)
            {
                //  Query Tas Zones,  that can be linked with SAM Spaces
                BuildingData buildingData = simulationData.GetBuildingData();
                Dictionary<string, ZoneData> dictionary_ZoneData = Query.ZoneDataDictionary(buildingData);

                // Our SAM Zones(list of Space GUIDs)
                foreach (Zone zone in zones)
                {
                    List<Space> spaces_Zone = adjacencyCluster.GetSpaces(zone);
                    if (spaces_Zone == null || spaces_Zone.Count == 0)
                        continue;

                    double area = adjacencyCluster.Sum(zone, Analytical.SpaceParameter.Area);
                    double volume = adjacencyCluster.Sum(zone, Analytical.SpaceParameter.Volume);
                    double occupancy = adjacencyCluster.Sum(zone, Analytical.SpaceParameter.Occupancy);

                    List<ZoneData> zoneDatas = new List<ZoneData>();
                    foreach (Space space in spaces_Zone)
                    {
                        string name = space?.Name;
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        List<SpaceSimulationResult> spaceSimulationResults_Space;
                        if (!dictionary.TryGetValue(space.Guid, out spaceSimulationResults_Space) || spaceSimulationResults_Space == null || spaceSimulationResults_Space.Count == 0)
                            continue;

                        SpaceSimulationResult spaceSimulationResult = spaceSimulationResults_Space[0];
                        if (spaceSimulationResult == null || string.IsNullOrWhiteSpace(spaceSimulationResult.Reference))
                            continue;

                        ZoneData zoneData = dictionary_ZoneData[spaceSimulationResult.Reference];
                        if (zoneData == null)
                            continue;

                        zoneDatas.Add(zoneData);
                    }

                    int index;
                    double max;

                    //Cooling
                    ZoneSimulationResult zoneSimulationResult_Cooling = null;
                    if (buildingData.TryGetMax(zoneDatas.ConvertAll(x => x.zoneGUID), tsdZoneArray.coolingLoad, out index, out max) && index != -1 && !double.IsNaN(max))
                    {
                        zoneSimulationResult_Cooling = new ZoneSimulationResult(zone.Name, Assembly.GetExecutingAssembly().GetName()?.Name, zone.Guid.ToString());
                        zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.MaxSensibleLoad, max);
                        zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.MaxSensibleLoadIndex, index);
                        zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.LoadType, LoadType.Cooling.Text());

                        List<SpaceSimulationResult> spaceSimulationResults_Zone = new List<SpaceSimulationResult>();
                        foreach(ZoneData zoneData in zoneDatas)
                        {
                            SpaceSimulationResult spaceSimulationResult_Temp = Create.SpaceSimulationResult(zoneData, index, LoadType.Cooling, SizingMethod.Simulation);
                            if (spaceSimulationResult_Temp == null)
                                continue;

                            spaceSimulationResults_Zone.Add(spaceSimulationResult_Temp);
                        }

                        if(spaceSimulationResults_Zone != null && spaceSimulationResults_Zone.Count != 0)
                        {
                            double airMovementGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.AirMovementGain);
                            if (!double.IsNaN(airMovementGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.AirMovementGain, airMovementGain);

                            double buildingHeatTransfer = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.BuildingHeatTransfer);
                            if (!double.IsNaN(buildingHeatTransfer))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.BuildingHeatTransfer, buildingHeatTransfer);

                            double equipmentSensibleGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.EquipmentSensibleGain);
                            if (!double.IsNaN(equipmentSensibleGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.EquipmentSensibleGain, equipmentSensibleGain);

                            double glazingExternalConduction = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.GlazingExternalConduction);
                            if (!double.IsNaN(glazingExternalConduction))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.GlazingExternalConduction, glazingExternalConduction);

                            double lightingGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.LightingGain);
                            if (!double.IsNaN(lightingGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.LightingGain, lightingGain);

                            double infiltrationGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.InfiltrationGain);
                            if (!double.IsNaN(infiltrationGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.InfiltrationGain, infiltrationGain);

                            double occupancySensibleGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.OccupancySensibleGain);
                            if (!double.IsNaN(occupancySensibleGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.OccupancySensibleGain, occupancySensibleGain);

                            double opaqueExternalConduction = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.OpaqueExternalConduction);
                            if (!double.IsNaN(opaqueExternalConduction))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.OpaqueExternalConduction, opaqueExternalConduction);

                            double solarGain = spaceSimulationResults_Zone.Sum(Analytical.SpaceSimulationResultParameter.SolarGain);
                            if (!double.IsNaN(solarGain))
                                zoneSimulationResult_Cooling.SetValue(ZoneSimulationResultParameter.SolarGain, solarGain);
                        }
                    }

                    if (!double.IsNaN(occupancy))
                        zoneSimulationResult_Cooling?.SetValue(ZoneSimulationResultParameter.Occupancy, occupancy);

                    if (!double.IsNaN(area))
                        zoneSimulationResult_Cooling?.SetValue(ZoneSimulationResultParameter.Area, area);

                    if (!double.IsNaN(volume))
                        zoneSimulationResult_Cooling?.SetValue(ZoneSimulationResultParameter.Volume, volume);


                    if(zoneSimulationResult_Cooling != null)
                    {
                        adjacencyCluster.AddObject(zoneSimulationResult_Cooling);
                        adjacencyCluster.AddRelation(zone, zoneSimulationResult_Cooling);
                        result.Add(zoneSimulationResult_Cooling);
                    }

                    //Heating
                    //ZoneSimulationResult zoneSimulationResult_Heating = null;
                    //if (buildingData.TryGetMax(zoneDatas.ConvertAll(x => x.zoneGUID), tsdZoneArray.heatingLoad, out index, out max) && index != -1 && !double.IsNaN(max))
                    //{
                        //zoneSimulationResult_Heating = new ZoneSimulationResult(zone.Name, Assembly.GetExecutingAssembly().GetName()?.Name, zone.Guid.ToString());
                        //zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.MaxSensibleLoad, max);
                        //zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.MaxSensibleLoadIndex, index);
                        //zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.LoadType, LoadType.Heating.Text());

                        //List<SpaceSimulationResult> spaceSimulationResults_Zone = new List<SpaceSimulationResult>();
                        //foreach (ZoneData zoneData in zoneDatas)
                        //{
                        //    SpaceSimulationResult spaceSimulationResult_Temp = spaceSimulationResults.FindAll(x => x.LoadType() == LoadType.Heating).Find(x => x.Reference == zoneData.zoneGUID);
                        //    if (spaceSimulationResult_Temp == null)
                        //        continue;

                        //    spaceSimulationResults_Zone.Add(spaceSimulationResult_Temp);
                        //}

                        //if (spaceSimulationResults_Zone != null && spaceSimulationResults_Zone.Count != 0)
                        //{
                        //    //double senisbleLoad = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.Load);
                        //    //if (!double.IsNaN(senisbleLoad))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.MaxSensibleLoad, senisbleLoad);

                        //    //double airMovementGain = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.AirMovementGain);
                        //    //if (!double.IsNaN(airMovementGain))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.AirMovementGain, airMovementGain);

                        //    //double buildingHeatTransfer = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.BuildingHeatTransfer);
                        //    //if (!double.IsNaN(buildingHeatTransfer))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.BuildingHeatTransfer, buildingHeatTransfer);

                        //    //double glazingExternalConduction = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.GlazingExternalConduction);
                        //    //if (!double.IsNaN(glazingExternalConduction))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.GlazingExternalConduction, glazingExternalConduction);

                        //    //double infiltrationGain = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.InfiltrationGain);
                        //    //if (!double.IsNaN(infiltrationGain))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.InfiltrationGain, infiltrationGain);

                        //    //double opaqueExternalConduction = spaceSimulationResults_Zone.Sum(SpaceSimulationResultParameter.OpaqueExternalConduction);
                        //    //if (!double.IsNaN(opaqueExternalConduction))
                        //    //    zoneSimulationResult_Heating.SetValue(ZoneSimulationResultParameter.OpaqueExternalConduction, opaqueExternalConduction);
                        //}
                    //}

                    //if (!double.IsNaN(occupancy))
                    //    zoneSimulationResult_Heating?.SetValue(ZoneSimulationResultParameter.Occupancy, occupancy);

                    //if (!double.IsNaN(area))
                    //    zoneSimulationResult_Heating?.SetValue(ZoneSimulationResultParameter.Area, area);

                    //if (!double.IsNaN(volume))
                    //    zoneSimulationResult_Heating?.SetValue(ZoneSimulationResultParameter.Volume, volume);


                    //if (zoneSimulationResult_Heating != null)
                    //{
                    //    adjacencyCluster.AddObject(zoneSimulationResult_Heating);
                    //    adjacencyCluster.AddRelation(zone, zoneSimulationResult_Heating);
                    //    result.Add(zoneSimulationResult_Heating);
                    //}
                }
            }

            return result;
        }
    }
}