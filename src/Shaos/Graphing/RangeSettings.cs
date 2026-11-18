namespace Shaos.Graphing
{
    /// <summary>
    /// The graph range settings
    /// </summary>
    public record RangeSettings
    {
        /// <summary>
        /// The minimum value
        /// </summary>
        public float Minimum { get; set; }

        /// <summary>
        /// The maximum value
        /// </summary>
        public float Maximum { get; set; }
    }
}