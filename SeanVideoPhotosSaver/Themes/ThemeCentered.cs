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
    public class ThemeCentered : ThemeBase
    {
        private const double DESIRED_SCALE_FACTOR = 0.8;
        private const double BORDER_THICKNESS = 20.0;

        public ThemeCentered(Canvas drawingCanvas) : base(drawingCanvas, 1)
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

            // Center it
            double newWidth = width * scaleValue + 2 * BORDER_THICKNESS;
            double newHeight = height * scaleValue + 2 * BORDER_THICKNESS;
            Canvas.SetLeft(border, (_drawingCanvas.ActualWidth - newWidth) / 2);
            Canvas.SetTop(border, (_drawingCanvas.ActualHeight - newHeight) / 2);
        }
    }
}
