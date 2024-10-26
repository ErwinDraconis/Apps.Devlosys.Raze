using System;
using System.IO;
using System.Reflection;

namespace Apps.Devlosys.Infrastructure
{
    public static class AppConsts
    {
        public static string Version => Assembly.GetCallingAssembly().GetName().Version.ToString(3);
    }

    public static class AppFolders
    {
        static AppFolders()
        {
            Directory.CreateDirectory(Pictures);
            Directory.CreateDirectory(Logs);
            Directory.CreateDirectory(Temp);
        }

        /// <summary>
        /// It represents the path where the "App.Windows.exe" is located.
        /// </summary>
        public static readonly string Base = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// It represents the path where the current assembly (*.dll file) is located.
        /// </summary>
        public static readonly string CurrentAssembly = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// %AppData%\Devlosys\App
        /// </summary>
        public static readonly string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Modus");

        /// <summary>
        /// %User%\Documents\Pictures
        /// </summary>
        public static readonly string Pictures = Path.Combine(CurrentAssembly, nameof(Pictures));

        /// <summary>
        /// %AppData%\Devlosys\App\Logs
        /// </summary>
        public static readonly string Logs = Path.Combine(AppData, nameof(Logs));

        /// <summary>
        /// %AppData%\Devlosys\App\Temp
        /// </summary>
        public static readonly string Temp = Path.Combine(AppData, nameof(Temp));
    }
}
