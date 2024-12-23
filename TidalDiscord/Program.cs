
namespace TidalDiscord
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength) => string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new TidalDiscordForm());
        }
    }
}
