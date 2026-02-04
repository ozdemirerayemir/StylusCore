using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StylusCore.App.Dialogs
{
    /// <summary>
    /// Simple custom input dialog that supports Dark Mode and theming.
    /// Replaces standard MessageBox or VB InputBox.
    /// </summary>
    public class InputDialog : Window
    {
        private TextBox _inputBox;
        public string InputText => _inputBox.Text;

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 400;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            
            // Apply modern WindowChrome
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 32,
                ResizeBorderThickness = new Thickness(0),
                GlassFrameThickness = new Thickness(1),
                CornerRadius = new CornerRadius(0),
                UseAeroCaptionButtons = true
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
            
            // Use DynamicResource for background to support theme switching
            this.SetResourceReference(BackgroundProperty, "PrimaryBackgroundBrush");

            var grid = new Grid { Margin = new Thickness(24) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(0, 0, 0, 8)
            };
            label.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
            
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            _inputBox = new TextBox
            {
                Height = 32,
                Padding = new Thickness(8, 6, 8, 6),
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
                FontSize = 14
            };
            // TextBox style is now handled by StandardControls.xaml via implicit style
            
            Grid.SetRow(_inputBox, 1);
            grid.Children.Add(_inputBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetRow(buttonPanel, 2);

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 32,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };
            okButton.SetResourceReference(StyleProperty, "PrimaryButtonStyle");
            okButton.Click += (s, e) => { DialogResult = true; };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 32,
                IsCancel = true
            };
            cancelButton.SetResourceReference(StyleProperty, "SecondaryButtonStyle");
            cancelButton.Click += (s, e) => { DialogResult = false; };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Content = grid;

            Loaded += (s, e) => _inputBox.Focus();
        }

        public void SetInputText(string text)
        {
            _inputBox.Text = text;
            _inputBox.SelectAll();
        }
    }
}
