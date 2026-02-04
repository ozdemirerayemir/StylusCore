using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StylusCore.App.Dialogs
{
    public partial class ColorPickerDialog : Window
    {
        public Color SelectedColor { get; private set; }

        public ColorPickerDialog()
        {
            InitializeComponent();
            InitializeColors();
        }

        private void InitializeColors()
        {
            var colors = new List<Color>
            {
                (Color)ColorConverter.ConvertFromString("#FF0000"), // Red
                (Color)ColorConverter.ConvertFromString("#FFA500"), // Orange
                (Color)ColorConverter.ConvertFromString("#FFFF00"), // Yellow
                (Color)ColorConverter.ConvertFromString("#008000"), // Green
                (Color)ColorConverter.ConvertFromString("#0000FF"), // Blue
                (Color)ColorConverter.ConvertFromString("#4B0082"), // Indigo
                (Color)ColorConverter.ConvertFromString("#EE82EE"), // Violet
                (Color)ColorConverter.ConvertFromString("#000000"), // Black
                (Color)ColorConverter.ConvertFromString("#FFFFFF"), // White
                (Color)ColorConverter.ConvertFromString("#808080"), // Gray
                (Color)ColorConverter.ConvertFromString("#A52A2A"), // Brown
                (Color)ColorConverter.ConvertFromString("#FFC0CB"), // Pink
                (Color)ColorConverter.ConvertFromString("#00CED1"), // Dark Turquoise
                (Color)ColorConverter.ConvertFromString("#FFD700"), // Gold
                (Color)ColorConverter.ConvertFromString("#FF4500"), // OrangeRed
                (Color)ColorConverter.ConvertFromString("#800000")  // Maroon
            };
            ColorList.ItemsSource = colors;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Color color)
            {
                SelectedColor = color;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
