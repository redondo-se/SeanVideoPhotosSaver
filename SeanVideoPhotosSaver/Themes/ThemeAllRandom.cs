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
    public class ThemeAllRandom : ThemeBase
    {
        private const double DESIRED_SCALE_FACTOR = 0.7;

        public ThemeAllRandom(Canvas drawingCanvas) : base(drawingCanvas, 8)
        {
        }

        protected override void DoCustomLayout(double width, double height)
        {
            FrameworkElement element = _currentItem;
            Border border = element.Parent as Border;
            border.BorderThickness = new Thickness(DEFAULT_BORDER_THICKNESS);
            border.BorderBrush = Brushes.White;

            // Create transform group for border (and nested item)
            TransformGroup tranformGroup = new TransformGroup();

            // Rotation
            double rotationAngle = RandomGenerator.Instance.Next(5, 16);
            double radians = Math.PI/180 * rotationAngle;
            rotationAngle = RandomGenerator.Instance.Next(0, 2) == 0 ? rotationAngle : rotationAngle * -1;
            tranformGroup.Children.Add(new RotateTransform(rotationAngle));

            // Calculate resulting width and height based on scaling and rotation
            double newWidth =  (width  + 2 * DEFAULT_BORDER_THICKNESS) * Math.Cos(radians) +
                               (height + 2 * DEFAULT_BORDER_THICKNESS) * Math.Sin(radians);

            double newHeight = (height + 2 * DEFAULT_BORDER_THICKNESS) * Math.Cos(radians) +
                               (width  + 2 * DEFAULT_BORDER_THICKNESS) * Math.Sin(radians);

            // Compute scaling
            double scaleValue = GetDesiredScaleFactor(newWidth, newHeight, DESIRED_SCALE_FACTOR);
            // Don't scale the border, just the image/video
            element.LayoutTransform = new ScaleTransform(scaleValue, scaleValue);

            // Fix height and width based on scaling
            newWidth =  (width  * scaleValue + 2 * DEFAULT_BORDER_THICKNESS) * Math.Cos(radians) +
                        (height * scaleValue + 2 * DEFAULT_BORDER_THICKNESS) * Math.Sin(radians);

            newHeight = (height * scaleValue + 2 * DEFAULT_BORDER_THICKNESS) * Math.Cos(radians) +
                        (width  * scaleValue + 2 * DEFAULT_BORDER_THICKNESS) * Math.Sin(radians);

            // Randomly place item on canvas
            Canvas.SetLeft(border, RandomGenerator.Instance.Next(0, (int)(_drawingCanvas.ActualWidth - newWidth)));
            Canvas.SetTop(border, RandomGenerator.Instance.Next(0, (int)(_drawingCanvas.ActualHeight - newHeight)));

            border.LayoutTransform = tranformGroup;
        }
    }
}
