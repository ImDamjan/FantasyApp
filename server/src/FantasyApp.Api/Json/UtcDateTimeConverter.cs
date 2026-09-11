using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasyApp.Api.Json
{
    /// <summary>
    /// Every DateTime in this app is conceptually UTC (synced from the FPL API or set via
    /// DateTime.UtcNow), but SQL Server's `datetime` columns don't preserve DateTimeKind — EF Core
    /// always returns Kind=Unspecified after a round trip. Without this converter, the default
    /// System.Text.Json output omits the 'Z' suffix for Unspecified values, and browsers then parse
    /// that string as local time instead of UTC, shifting every deadline by the client's UTC offset.
    /// This converter always treats DateTime as UTC on both read and write.
    /// </summary>
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetDateTime();
            return value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utc = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
            writer.WriteStringValue(utc);
        }
    }
}
