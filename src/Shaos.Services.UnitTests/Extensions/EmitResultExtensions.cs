
using Microsoft.CodeAnalysis.Emit;
using System.Text;

namespace Shaos.Services.UnitTests.Extensions
{
    internal static class EmitResultExtensions
    {
        public static string ToFormattedString(this EmitResult result)
        {
            StringBuilder builder = new();

            builder.AppendLine($"Result: [{result.Success}]");

            foreach (var diagnostic in result.Diagnostics)
            {
                builder.AppendLine($"Diagnostic: [{diagnostic.ToString()}]");
            }

            return builder.ToString();
        }
    }
}
