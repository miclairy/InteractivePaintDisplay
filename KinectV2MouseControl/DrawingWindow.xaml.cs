using System.Windows;
using System;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Kinect;
using System.Windows.Media;

namespace KinectV2MouseControl
{
    public partial class DrawingWindow: Window
    {
        MouseControl mouseControl = new MouseControl();
		KinectControl kinectControl = new KinectControl();
        private Point previousPoint;
		private bool draw = false;
		private BitmapImage handClipArt = new BitmapImage(new Uri("pack://application:,,,/hand.bmp", UriKind.Absolute));
		private Image handImg = new Image();
		private KinectSensor kinectSensor;
		private ColorFrameReader colorFrameReader;
		private WriteableBitmap colorBitmap = null;

		public DrawingWindow()
        {
           
			this.kinectSensor = KinectSensor.GetDefault();
			this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
			// wire handler for frame arrival
			this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

			// create the colorFrameDescription from the ColorFrameSource using Bgra format
			FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

			// create the bitmap to display
			this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

			// open the sensor
			this.kinectSensor.Open();

			// use the window object as the view model in this simple example
			this.DataContext = this;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            draw = true;
            previousPoint = e.GetPosition(this); //MouseControl.GetCursorPosition();
        }

		private void OnWaveForUserControl(object sender, EventArgs e)
		{
			draw = true;
		}

        private void drawArea_MouseMove(object sender, MouseEventArgs e)
        {
			Console.WriteLine("draw: " + kinectControl.Draw());
            if (kinectControl.Draw())
            {
				if (!drawArea.Children.Contains(handImg))
				{
					handImg.Source = handClipArt;
					handImg.Width = handClipArt.Width / 2;
					handImg.Height = handClipArt.Height / 2;
					drawArea.Children.Add(handImg);
				}
                Point currentPoint = e.GetPosition(this);// MouseControl.GetCursorPosition();
				Canvas.SetLeft(handImg, currentPoint.X + handImg.Width / 2);
				Canvas.SetTop(handImg, currentPoint.Y + handImg.Height / 2);

				if (previousPoint != null)
				{
					Line line = new Line();
					line.Stroke = System.Windows.Media.Brushes.DeepPink;
					line.X1 = previousPoint.X;
					line.X2 = currentPoint.X;
					line.Y1 = previousPoint.Y;
					line.Y2 = currentPoint.Y;
					line.StrokeThickness = 3;

					drawArea.Children.Add(line);
				}
				previousPoint = currentPoint;
            }

        }

        private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            draw = false;
        }

		private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
		{
			// ColorFrame is IDisposable
			using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
			{
				if (colorFrame != null)
				{
					FrameDescription colorFrameDescription = colorFrame.FrameDescription;

					using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
					{
						this.colorBitmap.Lock();

						// verify data and write the new color frame data to the display bitmap
						if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
						{
							colorFrame.CopyConvertedFrameDataToIntPtr(
								this.colorBitmap.BackBuffer,
								(uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
								ColorImageFormat.Bgra);

							this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
						}

						this.colorBitmap.Unlock();
					}
				}
			}
		}

		public ImageSource ImageSource
		{
			get
			{
				return this.colorBitmap;
			}
		}
	}


}
