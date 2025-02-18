using System.Text.Json.Serialization;
using System.Text.Json;

namespace Shaos.Json
{
    internal static class JsonSerializerOptionsDefault
    {
        private static Lazy<JsonSerializerOptions> _options = new Lazy<JsonSerializerOptions>(() =>
        {
            JsonSerializerOptions options = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        });

        public static JsonSerializerOptions Default { get; } = _options!.Value;

        public static void Configure(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
        }
    }
}
