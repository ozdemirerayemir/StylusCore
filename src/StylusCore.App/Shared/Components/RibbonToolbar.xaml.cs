using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using StylusCore.Core.Enums;

namespace StylusCore.App.Shared.Components
{
    /// <summary>
    /// Ribbon toolbar with PowerPoint-style collapsible content panel.
    /// </summary>
    public partial class RibbonToolbar : UserControl
    {
        private bool _isContentExpanded = false;
        private const double ContentHeight = 84;

        public event EventHandler ContentExpandedChanged;

        public RibbonToolbar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Whether the content panel is currently expanded
        /// </summary>
        public bool IsContentExpanded
        {
            get => _isContentExpanded;
            private set
            {
                if (_isContentExpanded != value)
                {
                    _isContentExpanded = value;
                    ContentExpandedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Get the current ribbon height (for layout calculations)
        /// </summary>
        public double CurrentHeight => IsContentExpanded ? 36 + ContentHeight : 36;

        private void Tab_Click(object sender, RoutedEventArgs e)
        {
            // Toggle content panel when clicking a tab
            if (IsContentExpanded)
            {
                // If clicking the same tab, collapse
                CollapseContent();
            }
            else
            {
                ExpandContent();
            }
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            // Handle tab switching - show appropriate content
            if (sender is RadioButton tab)
            {
                // For now, we only have File tab content
                // TODO: Add Edit, View, Tools tab content panels
            }
        }

        /// <summary>
        /// Expand the content panel with slide animation
        /// </summary>
        public void ExpandContent()
        {
            if (IsContentExpanded) return;

            var animation = new DoubleAnimation
            {
                To = ContentHeight,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ContentPanel.BeginAnimation(HeightProperty, animation);
            IsContentExpanded = true;
        }

        /// <summary>
        /// Collapse the content panel with slide animation
        /// </summary>
        public void CollapseContent()
        {
            if (!IsContentExpanded) return;

            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            ContentPanel.BeginAnimation(HeightProperty, animation);
            IsContentExpanded = false;

            // Uncheck all tabs when collapsing
            FileTab.IsChecked = false;
            EditTab.IsChecked = false;
            ViewTab.IsChecked = false;
            ToolsTab.IsChecked = false;
        }
    }
}
