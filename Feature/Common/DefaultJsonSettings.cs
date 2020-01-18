using System.Text.Json;

namespace HAS.Registration
{
    public static class DefaultJsonSettings
    {
        public static readonly JsonSerializerOptions Settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}
