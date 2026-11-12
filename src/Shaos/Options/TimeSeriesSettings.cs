using System.ComponentModel.DataAnnotations;

namespace Shaos.Options
{
    /// <summary>
    /// The time series settings
    /// </summary>
    public class TimeSeriesSettings
    {
        /// <summary>
        /// The time series fill colour
        /// </summary>
        public string FillColour { get; set; } = "rgba(0,128,255,0.5)";

        /// <summary>
        /// Fill from the time series line to the bottom of the graph
        /// </summary>
        public bool FillToBottom { get; set; } = false;

        /// <summary>
        /// The time series line colour
        /// </summary>
        public string LineColour { get; set; } = "rgba(0,128,255,0.5)";

        /// <summary>
        /// The time series line width setting
        /// </summary>
        [Range(1, 10)]
        public int LineWidth { get; set; } = 2;
    }
}