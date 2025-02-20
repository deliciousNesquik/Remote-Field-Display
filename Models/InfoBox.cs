using System;
using System.Globalization;


namespace RFD.Models
{
    public class InfoBox
    {
        /// <summary>Заголовок информационного блока</summary>
        public string Title { get; set; }
        /// <summary>Содержимое информационного блока</summary>
        public string Content { get; set; }
        /// <summary>Нижняя подпись информационного блока</summary>
        public string Inscription { get; set; }

        
        /// <summary>
        /// Конструктор создающий класс информационного блока
        /// </summary>
        /// <param name="title">Заголовок информационного блока</param>
        /// <param name="content">Содержимое информационного блока</param>
        /// <param name="inscription">Нижняя подпись информационного блока</param>
        public InfoBox(string title, object content, string inscription = "")
        {
            Title = title;
            Content = FormatContent(content);
            Inscription = inscription;
        }

        /// <summary>
        /// Форматирует переданное значение в строку
        /// </summary>
        /// <param name="content">Любой контент для отображения в содержимом информационного блока</param>
        /// <returns></returns>
        private static string FormatContent(object content)
        {
            return content switch
            {
                double d => d.ToString("F2", CultureInfo.CurrentCulture),
                float f => f.ToString("F2", CultureInfo.CurrentCulture),
                int i => i.ToString(CultureInfo.CurrentCulture),
                string s => TruncateString(s, 5),
                _ => content.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Обрезает строку, если она длиннее maxLength, и добавляет многоточие
        /// </summary>
        /// <param name="text">Переданный текст</param>
        /// <param name="maxLength">Максимальная длинна текста</param>
        /// <returns></returns>
        private static string TruncateString(string text, int maxLength)
        {
            return text.Length > maxLength ? string.Concat(text.AsSpan(0, maxLength), "...") : text;
        }
        
        /// <summary>
        /// Возвращает строковое представление объекта
        /// </summary>
        public override string ToString() => $"{Title}: {Content} ({Inscription})";
    }
}