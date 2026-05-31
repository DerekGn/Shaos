namespace Shaos.Pages.Parameters.Types
{
    public class ParameterHistory
    {
        /// <summary>
        /// The parameter history units
        /// </summary>
        public string? Units { get; set; }

        /// <summary>
        /// The parameter name
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The parameter history values
        /// </summary>
        public List<BaseHistoryValue> Values { get; set; } = [];
    }
}