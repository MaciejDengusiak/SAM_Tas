﻿using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical.Tas
{
    [AssociatedTypes(typeof(Panel)), Description("Panel Parameter")]
    public enum ApertureParameter
    {
        [ParameterProperties("Frame Zone Surface Guid", "Frame Zone Surface Guid"), ParameterValue(Core.ParameterType.String)] FrameZoneSurfaceGuid,
        [ParameterProperties("Pane Zone Surface Guid", "Pane Zone Surface Guid"), ParameterValue(Core.ParameterType.String)] PaneZoneSurfaceGuid,
    }
}