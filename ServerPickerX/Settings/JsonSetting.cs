
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
    public class JsonSetting : Setting
    {
        public string server_revision { get; set; } = "-1";

        public bool version_check_on_startup { get; set; } = true;

        [JsonIgnore]
        public readonly string jsonFileName = "settings.json";

        [JsonIgnore]
        private readonly JsonSerializerOptions serializerOptions = new(){ WriteIndented = true };

        public override async Task<Setting> LoadSettings()
        {
            // publishing this app with trimmed assemblies for reduced app size limits the
            // functionality of serialization since it requires reflection to determine
            // dynamic types on runtime which is not possible with trimmed applications
            // that pre-determines static types of the app during compilation
            try
            {
                if (!File.Exists(jsonFileName))
                {
                    FileStream file = File.Create(jsonFileName);

                    await JsonSerializer.SerializeAsync(file, this, serializerOptions);

                    file.Close();

                    return this;
                }

                FileStream settingsFile = File.OpenRead(jsonFileName);

                JsonSetting localSettings = await JsonSerializer.DeserializeAsync<JsonSetting>(settingsFile) ?? this;

                server_revision = localSettings.server_revision;
                version_check_on_startup = localSettings.version_check_on_startup;

                settingsFile.Close();
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", "An error has occured while loading json settings");

                await LogHelper.LogErrorToFile(ex.Message, "An error has occured while loading json settings");
            }

            return this;
        }

        public override async Task<bool> SaveSettings()
        {
            try
            {
                FileStream file = File.OpenWrite(jsonFileName);

                await JsonSerializer.SerializeAsync(file, this, serializerOptions);

                file.Close();

                return true;
            }
            catch (Exception ex) {
                await MessageBoxHelper.ShowMessageBox("Error", "An error has occured while saving json settings");

                await LogHelper.LogErrorToFile(ex.Message, "An error has occured while saving json settings");

                return false;
            }
        }
    }
}
