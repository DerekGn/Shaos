
namespace Shaos.Api.Model.v1
{
    /// <summary>
    /// The compilation result
    /// </summary>
    public class CompilationResult
    {
        /// <summary>
        /// The compiled assembly file path
        /// </summary>
        public string? AssemblyFilePath { get; set; }

        /// <summary>
        /// Indicates if this <see cref="CompilationResult"/> is successful
        /// </summary>
        public bool? Success { get; set; }

        /// <summary>
        /// The list of diagnostic errors or warnings
        /// </summary>
        public List<string>? Diagnostics { get; init; } = null;
    }
}
