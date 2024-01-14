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
    public class ThemeFullScreen : ThemeBase
    {
        private const double DESIRED_SCALE_FACTOR = 1.0;

        public ThemeFullScreen(Canvas drawingCanvas) : base(drawingCanvas, 1)
        {
            _drawingCanvas.Background = Brushes.Black;
        }

        protected override void DoCustomLayout(double width, double height)
        {
            Border border = _currentItem.Parent as Border;
            border.BorderThickness = new Thickness(0);

            // Compute scaling
            double scaleValue = GetDesiredScaleFactor(width, height, DESIRED_SCALE_FACTOR);
            // scale the image/video
            _currentItem.LayoutTransform = new ScaleTransform(scaleValue, scaleValue);

            // Center it
            Canvas.SetLeft(border, (_drawingCanvas.ActualWidth - width * scaleValue) / 2);
            Canvas.SetTop(border, (_drawingCanvas.ActualHeight - height * scaleValue) / 2);
        }
    }
}
