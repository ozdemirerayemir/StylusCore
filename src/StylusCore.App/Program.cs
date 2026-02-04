using System;

namespace StylusCore.App
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new StylusCore.App.App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"CRITICAL STARTUP ERROR:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "StylusCore Fatal Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
