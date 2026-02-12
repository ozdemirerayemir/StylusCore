using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StylusCore.Core.Enums;
using StylusCore.Core.Models;

namespace StylusCore.App.Features.Library.Dialogs
{
    /// <summary>
    /// Dialog for creating a new notebook with customization options.
    /// </summary>
    public partial class CreateNotebookDialog : Window
    {
        /// <summary>
        /// The created notebook if dialog result is true
        /// </summary>
        public Notebook CreatedNotebook { get; private set; }

        public CreateNotebookDialog()
        {
            InitializeComponent();
            NameTextBox.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Validate name
            var name = NameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a notebook name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Create the notebook with selected options
            CreatedNotebook = new Notebook
            {
                Name = name,
                CoverColor = GetSelectedCoverColor(),
                DefaultPageFormat = GetSelectedPageFormat(),
                DefaultPageOrientation = GetSelectedOrientation(),
                DefaultPageTemplate = GetSelectedTemplate()
            };

            DialogResult = true;
            Close();
        }

        private CoverColor GetSelectedCoverColor()
        {
            var selectedColor = ColorPanel.Children.OfType<RadioButton>()
                .FirstOrDefault(rb => rb.IsChecked == true);

            if (selectedColor?.Tag is string colorName)
            {
                return colorName switch
                {
                    "Purple" => CoverColor.Purple,
                    "Blue" => CoverColor.Blue,
                    "Green" => CoverColor.Green,
                    "Yellow" => CoverColor.Yellow,
                    "Red" => CoverColor.Red,
                    "Pink" => CoverColor.Pink,
                    "Orange" => CoverColor.Orange,
                    "Teal" => CoverColor.Teal,
                    _ => CoverColor.Purple
                };
            }
            return CoverColor.Purple;
        }

        private PageFormat GetSelectedPageFormat()
        {
            return FormatComboBox.SelectedIndex switch
            {
                0 => PageFormat.A4,
                1 => PageFormat.A5,
                2 => PageFormat.A3,
                3 => PageFormat.Letter,
                4 => PageFormat.Infinite,
                _ => PageFormat.A4
            };
        }

        private PageOrientation GetSelectedOrientation()
        {
            return PortraitRadio.IsChecked == true
                ? PageOrientation.Portrait
                : PageOrientation.Landscape;
        }

        private PageTemplate GetSelectedTemplate()
        {
            if (TemplateBlank.IsChecked == true) return PageTemplate.Blank;
            if (TemplateLined.IsChecked == true) return PageTemplate.Lined;
            if (TemplateGrid.IsChecked == true) return PageTemplate.Grid;
            if (TemplateDotted.IsChecked == true) return PageTemplate.Dotted;
            if (TemplateCornell.IsChecked == true) return PageTemplate.Cornell;
            return PageTemplate.Blank;
        }
    }
}
