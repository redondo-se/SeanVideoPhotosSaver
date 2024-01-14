using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace SeanVideoPhotosSaver.Themes
{
    public class ThemeTwoDiagonal : ThemeBase
    {
        private const double DESIRED_SCALE_FACTOR = 0.7;
        private const double BORDER_THICKNESS = 20.0;
        private const double EDGE_MARGIN = 20.0;

        private int _lastQuadrant = 0;  // 1 is upper left, 2 is upper right, 3 is lower right, 4 is lower left

        public ThemeTwoDiagonal(Canvas drawingCanvas) : base(drawingCanvas, 2)
        {
        }

        protected override void DoCustomLayout(double width, double height)
        {
            FrameworkElement element = _currentItem;
            Border border = element.Parent as Border;
            border.BorderThickness = new Thickness(BORDER_THICKNESS);
            border.BorderBrush = Brushes.White;

            // Compute scaling
            double scaleValue = GetDesiredScaleFactor(width, height, DESIRED_SCALE_FACTOR);
            // scale the image/video
            element.LayoutTransform = new ScaleTransform(scaleValue, scaleValue);

            if (_drawingCanvas.Children.Count == 1)
            {
                _lastQuadrant = RandomGenerator.Instance.Next(1, 5);
            }

            // Pick the quadrant for placement
            switch (_lastQuadrant)
            {
                case 1:
                    // place in quadrant 3 (lower right)
                    Canvas.SetRight(border, EDGE_MARGIN);
                    Canvas.SetBottom(border, EDGE_MARGIN);
                    _lastQuadrant = 3;
                    break;
                case 2:
                    // place in quadrant 4 (lower left)
                    Canvas.SetLeft(border, EDGE_MARGIN);
                    Canvas.SetBottom(border, EDGE_MARGIN);
                    _lastQuadrant = 4;
                    break;
                case 3:
                    // place in quadrant 1 (upper left)
                    Canvas.SetLeft(border, EDGE_MARGIN);
                    Canvas.SetTop(border, EDGE_MARGIN);
                    _lastQuadrant = 1;
                    break;
                case 4:
                    // place in quadrant 2 (upper right)
                    Canvas.SetRight(border, EDGE_MARGIN);
                    Canvas.SetTop(border, EDGE_MARGIN);
                    _lastQuadrant = 2;
                    break;
            }
        }
    }
}
