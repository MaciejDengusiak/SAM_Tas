﻿using GH_IO.Serialization;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Tas.Properties;
using SAM.Analytical.Tas;
using SAM.Core.Grasshopper;
using System;
using System.Windows.Forms;

namespace SAM.Analytical.Grasshopper.Tas
{
    public class SAMAnalyticalTasResultType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e6728078-da0c-4e9d-ba51-702c8ba49bd8");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        private ResultType resultType =  ResultType.Undefined;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalTasResultType()
          : base("SAMAnalytical.ResultType", "SAMAnalytical.ResultType",
              "Select Result Type",
              "SAM", "Tas")
        {
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("ResultType", (int)resultType);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            int aIndex = -1;
            if (reader.TryGetInt32("ResultType", ref aIndex))
                resultType = (ResultType)aIndex;

            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            foreach (ResultType resultType in Enum.GetValues(typeof(ResultType)))
                Menu_AppendItem(menu, resultType.ToString(), Menu_PanelTypeChanged, true, resultType == this.resultType).Tag = resultType;
        }

        private void Menu_PanelTypeChanged(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is ApertureType)
            {
                //Do something with panelType
                this.resultType = (ResultType)item.Tag;
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ResultType", "ResultType", "SAM Analytical ResultType", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, resultType);
        }
    }
}