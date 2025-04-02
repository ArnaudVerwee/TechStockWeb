namespace TechStockWeb.Models
{
    public class Language
    {
        public static List<Language> Languages { get; set; }
        public static Dictionary<string, Language> LanguageDictionary { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        static Language()
        {
            InitLanguages();
        }

        public static void InitLanguages()
        {
            Languages = new List<Language>
        {
            new Language { Id = "en", Name = "English" },
            new Language { Id = "fr", Name = "Français" },
            new Language { Id = "nl", Name = "Nederlands" }
        };

            LanguageDictionary = Languages.ToDictionary(l => l.Id, l => l);
        }
    }
}
