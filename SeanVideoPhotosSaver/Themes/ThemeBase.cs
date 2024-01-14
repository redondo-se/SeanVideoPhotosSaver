using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Windows.Threading;
//using WPFMediaKit.DirectShow.Controls;

namespace SeanVideoPhotosSaver.Themes
{
    public abstract class ThemeBase
    {
        public const double DEFAULT_FADE_TIME_SEC = 3.0;
        public const double DEFAULT_IMAGE_DISPLAY_TIME = 7.0;
        public const double DEFAULT_FADED_OPACITY = 0.7;
        public const double DEFAULT_BORDER_THICKNESS = 10.0;
        private static readonly DateTime TIME_ZERO = new DateTime();

        protected Canvas _drawingCanvas;
        protected FrameworkElement _currentItem;
        
        private List<string> _fileHistory = new List<string>();
        private int _fileHistoryIndex = 0;
        private Storyboard _currentStoryboard;
        private DispatcherTimer _imageDisplayTimer;
        private int _maxConcurrentItems;
        private DateTime _imageStartTime = TIME_ZERO;
        private bool _playVideoSound;

        protected ThemeBase(Canvas drawingCanvas, int maxConcurrentItems)
        {
            _drawingCanvas = drawingCanvas;
            _maxConcurrentItems = maxConcurrentItems;
            _imageDisplayTimer = new DispatcherTimer();
            _imageDisplayTimer.Interval = TimeSpan.FromMilliseconds(500.0);
            _imageDisplayTimer.Tick += new EventHandler(_imageDisplayTimer_Tick);
            FileNameProvider.Instance.FileListLoadedCompleted += new FileNameProvider.FileListLoadCompleteDelegate(Instance_FileListLoadedCompleted);
            _drawingCanvas.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Backgrounds/Background" +
                                                                                RandomGenerator.Instance.Next(1,7) + ".jpg")));
                                                                                //"1" + ".jpg")));

            ShowMessage("LOADING");
        }

        private void ShowMessage(string message)
        {
            _drawingCanvas.Children.Clear();
            Viewbox vbox = new Viewbox();
            vbox.Stretch = Stretch.Fill;
            vbox.Width = _drawingCanvas.ActualWidth;
            vbox.Height = _drawingCanvas.ActualHeight;
            TextBlock tbox = new TextBlock();
            tbox.Text = message;
            tbox.Foreground = Brushes.White;
            vbox.Child = tbox;
            _drawingCanvas.Children.Add(vbox);
        }

        void Instance_FileListLoadedCompleted()
        {
            _drawingCanvas.Children.Clear();
            if (!_imageDisplayTimer.IsEnabled)
            {
                _imageDisplayTimer.Start();
            }
            AddNextRandomVideoOrImage();
        }

        protected double GetDesiredScaleFactor(double mediaWidth, double mediaHeight, double desiredScale)
        {
            double itemWidth = _currentItem.Width;

            double percentX = mediaWidth/_drawingCanvas.ActualWidth;
            double percentY = mediaHeight/_drawingCanvas.ActualHeight;

            double scaleValue;
            if (percentX > percentY)
            {
                scaleValue = desiredScale * _drawingCanvas.ActualWidth / mediaWidth;
            }
            else
            {
                scaleValue = desiredScale * _drawingCanvas.ActualHeight / mediaHeight;
            }

            return scaleValue;
        }

        protected void AddNextRandomVideoOrImage()
        {
            foreach (var item in _drawingCanvas.Children)
            {
                Border border = item as Border;
                if (border != null)
                {
                    MediaElement videoContainer = border.Child as MediaElement;
                    if (videoContainer != null)
                    {
                        // Pause video, replace it with image of video, then close video
                        videoContainer.Pause();
            
                        RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)videoContainer.DesiredSize.Width, (Int32)videoContainer.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
                        DrawingVisual dv = new DrawingVisual();
                        using (DrawingContext dc = dv.RenderOpen())
                        {
                            VisualBrush vb = new VisualBrush(videoContainer);
                            dc.DrawRectangle(vb, null, new Rect(new Point(), videoContainer.DesiredSize));
                        }
                        rtb.Render(dv);

                        Image videoImage = new Image();
                        videoImage.Source = rtb;
                        border.Child = videoImage;

                        videoContainer.Close();
                    }
                }
            }

            ClearCanvasIfNeeded();

            _imageStartTime = TIME_ZERO;

            string nextFileName;

            if (_fileHistoryIndex < _fileHistory.Count)
            {
                nextFileName = _fileHistory[_fileHistoryIndex];
            }
            else
            {
                nextFileName = FileNameProvider.Instance.GetNextFileName();
                if (nextFileName == null)
                {
                    if (FileNameProvider.Instance.LoadedFromCache)
                    {
                        ShowMessage("LOADING");
                        FileNameProvider.Instance.LoadFileListAsync();
                        return;
                    }
                    else if (_fileHistory.Count > 0)
                    {
                        if (_fileHistoryIndex >= _fileHistory.Count)
                        {
                            _fileHistoryIndex = 0;
                        }

                        nextFileName = _fileHistory[_fileHistoryIndex];
                    }
                }
                else
                {
                    _fileHistory.Add(nextFileName);
                }
            }
            ++_fileHistoryIndex;


            if (nextFileName != null)
            {
                string fileExt = MediaTypeChecker.Instance.GetFileExtension(nextFileName);

                if (MediaTypeChecker.Instance.IsExtensionVideo(fileExt))
                {
                    MediaElement videoContainer = new MediaElement();
                    videoContainer.SizeChanged += new SizeChangedEventHandler(videoContainer_SizeChanged);

                    videoContainer.BeginInit();
                    videoContainer.Volume = _playVideoSound ? 1.0 : 0.0;
                    videoContainer.Source = new Uri(nextFileName);
                    videoContainer.LoadedBehavior = MediaState.Manual;
                    videoContainer.Play();
                    videoContainer.Pause();
                    videoContainer.EndInit();

                    Border border = new Border();
                    border.Child = videoContainer;
                    _drawingCanvas.Children.Add(border);
                }
                else if (MediaTypeChecker.Instance.IsExtentionsImage(fileExt))
                {
                    Image newImage = new Image();
                    newImage.Source = LoadImageFile(nextFileName);

                    Border border = new Border();
                    border.Child = newImage;
                    _drawingCanvas.Children.Add(border);
                    AddNextItem(newImage, newImage.Source.Width, newImage.Source.Height);
                }
            }
        }

        private static string _orientationQuery = "System.Photo.Orientation";
        public static BitmapImage LoadImageFile(String path)
        {
            Rotation rotation = Rotation.Rotate0;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                BitmapMetadata bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;

                if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(_orientationQuery)))
                {
                    object o = bitmapMetadata.GetQuery(_orientationQuery);

                    if (o != null)
                    {
                        switch ((ushort)o)
                        {
                            case 6:
                                {
                                    rotation = Rotation.Rotate90;
                                }
                                break;
                            case 3:
                                {
                                    rotation = Rotation.Rotate180;
                                }
                                break;
                            case 8:
                                {
                                    rotation = Rotation.Rotate270;
                                }
                                break;
                        }
                    }
                }
            }

            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.UriSource = new Uri(path);
            bImg.Rotation = rotation;
            bImg.EndInit();
            bImg.Freeze();

            return bImg;
        }

        private void ClearCanvasIfNeeded()
        {
            if (_drawingCanvas.Children.Count >= _maxConcurrentItems)
            {
                FileNameProvider.Instance.SaveFileList();
                _drawingCanvas.Children.Clear();
            }
        }

        void videoContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MediaElement videoContainer = sender as MediaElement;
            videoContainer.Width = videoContainer.NaturalVideoWidth;
            videoContainer.Height = videoContainer.NaturalVideoHeight;
            videoContainer.SizeChanged -= new SizeChangedEventHandler(videoContainer_SizeChanged);
            AddNextItem(videoContainer, videoContainer.NaturalVideoWidth, videoContainer.NaturalVideoHeight);
        }
        
        private void AddNextItem(FrameworkElement element, double width, double height)
        {
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Completed -= new EventHandler(storyBoard_Completed);
                _currentStoryboard.Stop();
            }

            _currentStoryboard = new Storyboard();
            _currentStoryboard.Completed += new EventHandler(storyBoard_Completed);

            if (_currentItem != null)
            {
                // Add animation to fade out the last item
                if (_drawingCanvas.Children.Contains(_currentItem))
                {
                    DoubleAnimation fadeAway = new DoubleAnimation(1.0, DEFAULT_FADED_OPACITY, new Duration(TimeSpan.FromSeconds(DEFAULT_FADE_TIME_SEC)));
                    _currentStoryboard.Children.Add(fadeAway);
                    Storyboard.SetTarget(fadeAway, _currentItem);
                    Storyboard.SetTargetProperty(fadeAway, new PropertyPath("Opacity"));
                }
            }

            _currentItem = element;
            DoCustomLayout(width, height);

            DoubleAnimation fadeIn = new DoubleAnimation(DEFAULT_FADED_OPACITY, 1.0, new Duration(TimeSpan.FromSeconds(DEFAULT_FADE_TIME_SEC)));
            _currentStoryboard.Children.Add(fadeIn);
            Storyboard.SetTarget(fadeIn, element);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
            _currentStoryboard.Begin();
        }

        void storyBoard_Completed(object sender, EventArgs e)
        {
            _currentStoryboard = null;

            MediaElement videoContainer = GetCurrentItemAsMediaElement();

            if (videoContainer != null)
            {
                _imageStartTime = TIME_ZERO;
                videoContainer.MediaEnded += new RoutedEventHandler(MediaPlaybackFinished);
                videoContainer.Play();
            }
            else
            {
                _imageStartTime = DateTime.Now;
            }
        }

        void MediaPlaybackFinished(object sender, RoutedEventArgs e)
        {
            MediaElement videoContainer = sender as MediaElement;
            videoContainer.MediaEnded -= new RoutedEventHandler(MediaPlaybackFinished);
            AddNextRandomVideoOrImage();
        }

        void _imageDisplayTimer_Tick(object sender, EventArgs e)
        {
            if ((_imageStartTime != TIME_ZERO) && ((DateTime.Now - _imageStartTime).TotalSeconds > DEFAULT_IMAGE_DISPLAY_TIME))
            {
                _imageStartTime = TIME_ZERO;
                AddNextRandomVideoOrImage();
            }
        }

        public void MoveBackward()
        {
            _fileHistoryIndex -= 2;

            if (_fileHistoryIndex < 0)
            {
                _fileHistoryIndex = _fileHistory.Count - 1;
            }

            AddNextRandomVideoOrImage();
        }

        public void MoveForward()
        {
            if (_currentItem != null)
            {
                _currentItem.Opacity = DEFAULT_FADED_OPACITY;
            }
            AddNextRandomVideoOrImage();
        }

        public void CleanShutdown()
        {
            try
            {
                _imageDisplayTimer.Stop();

                if (_currentStoryboard != null)
                {
                    _currentStoryboard.Completed -= new EventHandler(storyBoard_Completed);
                    _currentStoryboard.Stop();
                }

                MediaElement videoContainer = GetCurrentItemAsMediaElement();
                if ((videoContainer != null))
                {
                    videoContainer.MediaEnded -= new RoutedEventHandler(MediaPlaybackFinished);
                    videoContainer.Stop();
                    videoContainer.Close();
                }
                
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private MediaElement GetCurrentItemAsMediaElement()
        {
            MediaElement videoContainer = null;
            if (_currentItem != null)
            {
                try
                {
                    videoContainer = ((Border)_currentItem).Child as MediaElement;
                }
                catch (Exception)
                {
                    videoContainer = _currentItem as MediaElement;
                }
            }

            return videoContainer;
        }

        public void ToggleMute()
        {
            _playVideoSound = !_playVideoSound;

            MediaElement videoContainer = GetCurrentItemAsMediaElement();
            if (videoContainer != null)
            {
                videoContainer.Volume = _playVideoSound ? 1.0 : 0.0;
            }
        }

        protected abstract void DoCustomLayout(double width, double height);
    }
}
