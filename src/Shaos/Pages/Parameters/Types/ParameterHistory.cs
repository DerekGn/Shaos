namespace Shaos.Pages.Parameters.Types
{
    public class ParameterHistory
    {
        /// <summary>
        /// The parameter name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The parameter history values
        /// </summary>
        public List<BaseHistoryValue> Values { get; set; } = [];
    }
}
