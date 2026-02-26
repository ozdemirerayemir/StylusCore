using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace StylusCore.App.Dialogs
{
    /// <summary>
    /// Modern input dialog matching the React "Create New Library" design.
    /// Features: rounded corners, styled title, placeholder textbox, Cancel/Create buttons.
    /// </summary>
    public class InputDialog : Window
    {
        private TextBox _inputBox;
        public string InputText => _inputBox.Text;

        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 460;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;

            // ────────────── OUTER CARD ──────────────
            var outerBorder = new Border
            {
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(0),
                Margin = new Thickness(24), // space for drop shadow
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 4,
                    Opacity = 0.15,
                    BlurRadius = 24
                }
            };
            outerBorder.SetResourceReference(Border.BackgroundProperty, "Brush.AppBg");

            var mainStack = new StackPanel { Margin = new Thickness(28, 24, 28, 24) };

            // ── HEADER ROW (Title + Close) ──
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            titleBlock.SetResourceReference(TextBlock.ForegroundProperty, "Brush.TextPrimary");
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            // Close (X) button
            var closeBtn = new Button
            {
                Content = "✕",
                Width = 28,
                Height = 28,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                IsCancel = true
            };
            closeBtn.SetResourceReference(Control.ForegroundProperty, "Brush.TextTertiary");
            closeBtn.Click += (s, e) => { DialogResult = false; };
            Grid.SetColumn(closeBtn, 1);
            headerGrid.Children.Add(closeBtn);
            mainStack.Children.Add(headerGrid);

            // ── LABEL ──
            var label = new TextBlock
            {
                Text = prompt,
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 8)
            };
            label.SetResourceReference(TextBlock.ForegroundProperty, "Brush.TextSecondary");
            mainStack.Children.Add(label);

            // ── INPUT TEXTBOX ──
            _inputBox = new TextBox
            {
                Height = 42,
                Padding = new Thickness(12, 8, 12, 8),
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24),
                FontSize = 14,
                BorderThickness = new Thickness(1.5)
            };
            _inputBox.SetResourceReference(Control.BorderBrushProperty, "Brush.Accent");
            mainStack.Children.Add(_inputBox);

            // ── BUTTON ROW ──
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            // Cancel button (Ghost style)
            var cancelButton = new Button
            {
                Content = FindResource("Str_Cancel") ?? "Cancel",
                MinWidth = 80,
                Height = 36,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.SetResourceReference(StyleProperty, "GhostButtonStyle");
            cancelButton.Click += (s, e) => { DialogResult = false; };
            buttonPanel.Children.Add(cancelButton);

            // Create button (Primary style)
            var createButton = new Button
            {
                Content = FindResource("Str_Create") ?? "Create",
                MinWidth = 90,
                Height = 36,
                FontSize = 14,
                Cursor = System.Windows.Input.Cursors.Hand,
                IsDefault = true
            };
            createButton.SetResourceReference(StyleProperty, "PrimaryButtonStyle");
            createButton.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_inputBox.Text))
                    DialogResult = true;
            };
            buttonPanel.Children.Add(createButton);

            mainStack.Children.Add(buttonPanel);
            outerBorder.Child = mainStack;
            Content = outerBorder;

            // Focus the textbox
            Loaded += (s, e) => _inputBox.Focus();

            // Allow dragging the dialog
            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                    DragMove();
            };
        }

        public void SetInputText(string text)
        {
            _inputBox.Text = text;
            _inputBox.SelectAll();
        }

        private object FindResource(string key)
        {
            try { return Application.Current.FindResource(key); }
            catch { return null; }
        }
    }
}
