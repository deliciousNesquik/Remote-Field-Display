using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RFD.Services
{
    public static class SettingsApplication
    {
        public static bool SaveData { get; set; } = true;
        public static string BrandColor { get; set; } = "#3e66ad";
        public static string BrandHoverColor { get; set; } = "#3e66ad";
        public static int Language { get; set; }
        public static Localization LanguageLocalization { get; set; }
        public static bool ThemeProtection { get; set; }

        // Путь к файлу рядом с исполняемым файлом
        private static readonly string SettingsPath = Path.Combine(
            AppContext.BaseDirectory, "settings.json"
        );

        public static void Save()
        {
            try
            {
                var settingsDto = new SettingsDTO
                {
                    SaveData = SaveData,
                    BrandColor = BrandColor,
                    BrandHoverColor = BrandHoverColor,
                    Language = Language,
                    LanguageLocalization = LanguageLocalization,
                    ThemeProtection = ThemeProtection
                };

                var json = JsonSerializer.Serialize(settingsDto, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true // если в Localization есть поля
                });

                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex}");
            }
        }


        public static void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return;

                var json = File.ReadAllText(SettingsPath);
                var settingsDto = JsonSerializer.Deserialize<SettingsDTO>(json);

                if (settingsDto != null)
                {
                    SaveData = settingsDto.SaveData;
                    BrandColor = settingsDto.BrandColor;
                    BrandHoverColor = BrandHoverColor;
                    Language = settingsDto.Language;
                    LanguageLocalization = settingsDto.LanguageLocalization;
                    ThemeProtection = ThemeProtection;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        // Вспомогательный DTO для сериализации
        private class SettingsDTO
        {
            public bool SaveData { get; set; }
            public string BrandColor { get; set; } = "#3e66ad";
            public string BrandHoverColor { get; set; } = "#3e66ad";
            public int Language { get; set; }
            public Localization LanguageLocalization { get; set; }
            public bool ThemeProtection { get; set; }
        }
    }
}
