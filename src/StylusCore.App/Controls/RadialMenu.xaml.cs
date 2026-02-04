using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using StylusCore.Core.Enums;
using StylusCore.Input.Bindings;

namespace StylusCore.App.Controls
{
    /// <summary>
    /// Circular radial menu control with customizable items.
    /// Supports Click and Hold activation modes.
    /// </summary>
    public partial class RadialMenu : UserControl
    {
        private RadialMenuConfig _config;
        private List<Button> _menuButtons;
        private RadialMenuActivation _activationMode;
        private bool _isHolding;
        private RadialMenuItem _hoveredItem;

        public event EventHandler<RadialMenuItem> ItemSelected;

        public RadialMenu()
        {
            InitializeComponent();
            _menuButtons = new List<Button>();
            _activationMode = RadialMenuActivation.Click;
        }

        /// <summary>
        /// Current menu configuration
        /// </summary>
        public RadialMenuConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                BuildMenu();
            }
        }

        /// <summary>
        /// Activation mode (Click or Hold)
        /// </summary>
        public RadialMenuActivation ActivationMode
        {
            get => _activationMode;
            set => _activationMode = value;
        }

        /// <summary>
        /// Show the menu at a specific position
        /// </summary>
        public void Show(Point position)
        {
            // Position menu centered on the point
            Margin = new Thickness(position.X - Width / 2, position.Y - Height / 2, 0, 0);
            Visibility = Visibility.Visible;

            // Animate in
            var scaleTransform = new ScaleTransform(0.5, 0.5, Width / 2, Height / 2);
            RenderTransform = scaleTransform;

            var scaleAnimation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        /// <summary>
        /// Hide the menu
        /// </summary>
        public void Hide()
        {
            // Animate out
            var scaleTransform = RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1, Width / 2, Height / 2);
            RenderTransform = scaleTransform;

            var scaleAnimation = new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(100))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            scaleAnimation.Completed += (s, e) => Visibility = Visibility.Collapsed;

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        /// <summary>
        /// Begin hold mode tracking
        /// </summary>
        public void StartHold()
        {
            _isHolding = true;
        }

        /// <summary>
        /// End hold mode and select hovered item
        /// </summary>
        public void EndHold()
        {
            if (_isHolding && _hoveredItem != null)
            {
                ItemSelected?.Invoke(this, _hoveredItem);
            }
            _isHolding = false;
            Hide();
        }

        /// <summary>
        /// Build the menu items from config
        /// </summary>
        private void BuildMenu()
        {
            ItemsCanvas.Children.Clear();
            _menuButtons.Clear();

            if (_config?.Items == null || _config.Items.Count == 0) return;

            CenterLabel.Text = _config.Name;

            int itemCount = _config.Items.Count;
            double angleStep = 360.0 / itemCount;
            double radius = 70; // Distance from center

            for (int i = 0; i < itemCount; i++)
            {
                var item = _config.Items[i];
                var angle = (i * angleStep - 90) * Math.PI / 180; // Start from top

                var button = new Button
                {
                    Style = FindResource("RadialMenuItemStyle") as Style,
                    Content = new TextBlock
                    {
                        Text = item.Icon ?? item.Label.Substring(0, 1),
                        FontSize = 16,
                        TextAlignment = TextAlignment.Center
                    },
                    ToolTip = item.Label,
                    Tag = item
                };

                // Position in circle
                double x = Width / 2 + radius * Math.Cos(angle) - 24; // 24 = half button width
                double y = Height / 2 + radius * Math.Sin(angle) - 24;

                Canvas.SetLeft(button, x);
                Canvas.SetTop(button, y);

                button.Click += MenuButton_Click;
                button.MouseEnter += MenuButton_MouseEnter;
                button.MouseLeave += MenuButton_MouseLeave;

                ItemsCanvas.Children.Add(button);
                _menuButtons.Add(button);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RadialMenuItem item)
            {
                ItemSelected?.Invoke(this, item);

                if (_activationMode == RadialMenuActivation.Click)
                {
                    Hide();
                }
            }
        }

        private void MenuButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Tag is RadialMenuItem item)
            {
                _hoveredItem = item;
                CenterLabel.Text = item.Label;
            }
        }

        private void MenuButton_MouseLeave(object sender, MouseEventArgs e)
        {
            _hoveredItem = null;
            if (_config != null)
            {
                CenterLabel.Text = _config.Name;
            }
        }
    }
}
