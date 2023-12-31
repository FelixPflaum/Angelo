using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Angelo.Keybinds;

namespace Angelo.Settings
{
    internal class KeyBindJsonConverter : JsonConverter<KeyBind>
    {
        public override KeyBind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            uint packed = reader.GetUInt32();
            KeyBind kb = KeyBind.FromPackedInt(packed);
            return kb;
        }

        public override void Write(Utf8JsonWriter writer, KeyBind value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.PackInt());
        }
    }
}
