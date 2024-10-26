using Apps.Devlosys.Core.Helpers;
using Apps.Devlosys.Infrastructure;
using Serilog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Apps.Devlosys.Core.Handlers
{
    public class ExceptionHandler
    {
        private readonly ILogger Logger = Serilog.Log.Logger;

        public void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log(e.ExceptionObject as Exception);
        }

        public void DispatcherUnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log(e.Exception);
        }

        private void Log(Exception exception)
        {
            Logger.Fatal("An uncaught exception occurred {ex}", exception);

            switch (exception)
            {
                case NotImplementedException _:
                    _ = MessageBox.Show(
                        "Sorry! The feature has NOT been IMPLEMENTED. Please wait for the next version. ",
                        "Fatal",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    break;
                case NotSupportedException _:
                    _ = MessageBox.Show(
                        "Sorry! The feature has NOT been SUPPORTED. Please wait for the next version. ",
                        "Fatal",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    break;
                default:
                    MessageBoxResult result = MessageBox.Show(
                        $"Sorry! An uncaught EXCEPTION occurred. {Environment.NewLine}" +
                        $"You can pack and send log files in %AppData%\\Accelerider\\Logs to the developer. Thank you! {Environment.NewLine}{Environment.NewLine}" +
                        $"Do you want to open the Logs folder? ",
                        "Fatal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Error);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(AppFolders.Logs);
                    }

                    break;
            }

            ProcessController.Restart(-1);
        }
    }
}
