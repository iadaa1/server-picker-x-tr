
using ServerPickerX.Helpers;
using ServerPickerX.Settings;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServerPickerX.ConfigSections
{
    // publishing an app with trimmed assemblies or using Ahead-of-Time compilation for 
    // reduced build size can limit the functionality of serialization since it requires reflection 
    // to determine dynamic types on runtime which is not possible with trimmed or AOT applications.
    // JsonSerializerContext preserves the types and provides serialization metadata on compile-time.
    [JsonSerializable(typeof(JsonSetting))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }

    public class JsonSetting : Setting
    {
        public string warning { get; private set; } = "Do not modify server revision! It will unblock all servers on launch";

        public string server_revision { get; set; } = "-1";

        public bool version_check_on_startup { get; set; } = true;

        [JsonIgnore]
        public readonly string jsonFilePath = "./settings.json";

        [JsonIgnore]
        public readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            TypeInfoResolver = SourceGenerationContext.Default,
            WriteIndented = true,        };

        public override async Task<Setting> LoadSettings()
        {
            try
            {
                // create local json settings if not exists with serialized object properties
                if (!File.Exists(jsonFilePath))
                {
                    using FileStream newSettingsFile = File.Create(jsonFilePath);

                    await JsonSerializer.SerializeAsync(newSettingsFile, this);

                    if (OperatingSystem.IsLinux())
                    {
                        await FileHelper.ChangeLinuxFileOwner(jsonFilePath);
                    }

                    return this;
                }

                using FileStream settingsFile = File.OpenRead(jsonFilePath);

                JsonSetting localSettings = await JsonSerializer.DeserializeAsync<JsonSetting>(settingsFile, serializerOptions) ?? this;

                server_revision = localSettings.server_revision;
                version_check_on_startup = localSettings.version_check_on_startup;
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", "An error has occured while loading json settings");

                await FileHelper.LogErrorToFile(ex.Message, "An error has occured while loading json settings");
            }

            return this;
        }

        public override async Task<bool> SaveSettings()
        {
            try
            {
                // an extra curly brace is being added in the json file when serializing,
                // remove the contents first then serialize data to file
                await File.WriteAllTextAsync(jsonFilePath, String.Empty);

                // open existing local json settings and deserialize it back to its complex form
                using FileStream file = File.OpenWrite(jsonFilePath);

                await JsonSerializer.SerializeAsync(file, this, serializerOptions);

                return true;
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", "An error has occured while saving json settings");

                await FileHelper.LogErrorToFile(ex.Message, "An error has occured while saving json settings");

                return false;
            }
        }
    }
}
