namespace PersonalDNSSinkhole
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainView());

            //Create personal blocklist file if it doesn't exist
            if (!File.Exists($"{Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}\\personalblocklist.txt"))
            {
                File.Create($"{Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName}\\personalblocklist.txt").Close();
            }
        }
    }
}