using System.Windows;
using System;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;
using System.Windows.Media;
using Microsoft.Kinect.Wpf.Controls;
using Microsoft.Kinect.Toolkit.Input;
using Microsoft.Kinect.Face;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace KinectV2InteractivePaint
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
		private CoordinateMapper coordinateMapper;
		private WriteableBitmap colorBitmap = null;
		private BodyFrameReader bodyFrameReader;
		private TimeSpan lastTime;
		private EngagementManager engagement;
		private int bodyCount;
		private Body[] bodies;
		private FaceFrameSource[] faceFrameSources = null;
		private FaceFrameReader[] faceFrameReaders = null;
		private FaceFrameResult[] faceFrameResults;

		private int displayWidth;
		private int displayHeight;

		private List<Polyline> drawingSegments = new List<Polyline>();
		private Polyline currentDrawingSegment;
		private Dictionary<int, List<Polyline>> drawingSegmentsFaces = new Dictionary<int, List<Polyline>>();
		private FaceFrameResult[] previousResults;
		Rectangle rect = new Rectangle()
		{
			HorizontalAlignment = HorizontalAlignment.Left,
			Height = 20,
			Width = 20,
			StrokeThickness = 5,
			Stroke = Brushes.Yellow
		};

		public DrawingWindow()
        {
			
			this.kinectSensor = KinectSensor.GetDefault();
			this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
			this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
			// create the colorFrameDescription from the ColorFrameSource using Bgra format
			FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
			this.coordinateMapper = this.kinectSensor.CoordinateMapper;
			this.displayWidth = colorFrameDescription.Width;
			this.displayHeight = colorFrameDescription.Height;

			// create the bitmap to display
			this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

			this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
			this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
			this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;
			this.bodies = new Body[this.bodyCount];
			//this.faceFrameSources = new FaceFrameSource[this.bodyCount];
			//this.faceFrameReaders = new FaceFrameReader[this.bodyCount];

			FaceFrameFeatures faceFrameFeatures =
				FaceFrameFeatures.BoundingBoxInColorSpace
				| FaceFrameFeatures.PointsInColorSpace
				| FaceFrameFeatures.RotationOrientation
				| FaceFrameFeatures.FaceEngagement
				| FaceFrameFeatures.Glasses
				| FaceFrameFeatures.Happy
				| FaceFrameFeatures.LeftEyeClosed
				| FaceFrameFeatures.RightEyeClosed
				| FaceFrameFeatures.LookingAway
				| FaceFrameFeatures.MouthMoved
				| FaceFrameFeatures.MouthOpen;

			this.faceFrameSources = new FaceFrameSource[this.bodyCount];
			this.faceFrameReaders = new FaceFrameReader[this.bodyCount];
			for (int i = 0; i < this.bodyCount; i++)
			{
				// create the face frame source with the required face frame features and an initial tracking Id of 0
				this.faceFrameSources[i] = new FaceFrameSource(this.kinectSensor, 0, faceFrameFeatures);

				// open the corresponding reader
				this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
			}

			// allocate storage to store face frame results for each face in the FOV
			this.faceFrameResults = new FaceFrameResult[this.bodyCount];
			this.previousResults = new FaceFrameResult[this.bodyCount];


			this.kinectSensor.Open();
			KinectCoreWindow kinectCoreWindow = KinectCoreWindow.GetForCurrentThread();
			kinectCoreWindow.PointerMoved += drawArea_PointerMove;

			// use the window object as the view model in this simple example
			this.DataContext = this;

			KinectRegion.SetKinectRegion(this, kinectRegion);

			// App app = ((App)Application.Current);
			// app.KinectRegion = kinectRegion;
			
			this.engagement = new EngagementManager(kinectSensor);
			InitializeComponent();

		}

	

		private void kinectRegion_Loaded(object sender, RoutedEventArgs e)
		{
			this.kinectRegion.KinectSensor = KinectSensor.GetDefault();
			this.kinectRegion.SetKinectOnePersonManualEngagement(engagement);
			this.kinectRegion.KinectEngagementManager.StartManaging();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
        {

			for (int i = 0; i < this.bodyCount; i++)
			{
				if (this.faceFrameReaders[i] != null)
				{
					// wire handler for face frame arrival
					this.faceFrameReaders[i].FrameArrived += this.Reader_FaceFrameArrived;
				}
			}

			this.engagement.Engaged += (s, args) =>
			{
				this.draw = true;
			};

			if (this.bodyFrameReader != null)
			{
				// wire handler for body frame arrival
				this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
			}

		}

		private void Reader_FaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
		{
			using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
			{
				
				if (faceFrame != null)
				{
					int index = this.GetFaceSourceIndex(faceFrame.FaceFrameSource);
					
					// check if this face frame has valid face frame results
					if (this.ValidateFaceBoxAndPoints(faceFrame.FaceFrameResult))
					{
						// store this face frame result to draw later
						
						this.faceFrameResults[index] = faceFrame.FaceFrameResult;
					}
					else
					{
						// indicates that the latest face frame result from this reader is invalid
						this.faceFrameResults[index] = null;
					}

				}
			}
		}

		private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
		{
			int index = -1;

			for (int i = 0; i < this.bodyCount; i++)
			{
				if (this.faceFrameSources[i] == faceFrameSource)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{

			using (var bodyFrame = e.FrameReference.AcquireFrame())
			{
				if (bodyFrame != null)
				{
					bodyFrame.GetAndRefreshBodyData(this.bodies);

					for (int i = 0; i < this.bodyCount; i++)
					{
						if (this.faceFrameSources[i].IsTrackingIdValid)
						{
							if (this.faceFrameResults[i] != null)
							{
								this.DrawFaceFrameResults(i, this.faceFrameResults[i]);
							}

						}
						else
						{
							if (this.bodies[i].IsTracked)
							{
								this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
							}
						}
					}
				}
			}

		}

		private void DrawFaceFrameResults(int faceIndex, FaceFrameResult faceResult)
		{
			
			
			// draw the face bounding box
			var faceBoxSource = faceResult.FaceBoundingBoxInColorSpace;
			
			if (previousResults[faceIndex] != null)
			{
				var previousFaceBox = previousResults[faceIndex].FaceBoundingBoxInColorSpace;
				double movementX = faceBoxSource.Left - previousFaceBox.Left;
				double movementY = faceBoxSource.Top - previousFaceBox.Top;
				if (drawingSegmentsFaces.ContainsKey(faceIndex))
				{
					for (int j = 0; j < drawingSegmentsFaces[faceIndex].Count; j++)
					{

						Console.WriteLine(drawingSegmentsFaces[faceIndex][j].Points);
						if (movementX != 0 || movementY != 0)
						{
							PointCollection polyline = drawingSegmentsFaces[faceIndex][j].Points;

							for (int i = 0; i < polyline.Count; i++)
							{
								polyline[i] = new Point(polyline[i].X + movementX, polyline[i].Y + movementY);

							}
						}
					}
					
				}
			}
			previousResults[faceIndex] = faceResult;


			Rectangle faceBox = new Rectangle()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				Height = faceBoxSource.Bottom - faceBoxSource.Top,
				Width = faceBoxSource.Right - faceBoxSource.Left,
				StrokeThickness = 5,
				Stroke = Brushes.Blue
			};

			Canvas.SetLeft(faceBox, faceBoxSource.Left );
			Canvas.SetTop(faceBox, faceBoxSource.Top );
			// drawArea.Children.Add(faceBox);

			string faceText = string.Empty;

			// extract face rotation in degrees as Euler angles
			if (faceResult.FaceRotationQuaternion != null)
			{
				int pitch, yaw, roll;
				ExtractFaceRotationInDegrees(faceResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
				faceText += "FaceYaw : " + yaw + "\n" +
							"FacePitch : " + pitch + "\n" +
							"FacenRoll : " + roll + "\n";
			}
			//Console.WriteLine(faceText);
			
		}

		private bool ValidateFaceBoxAndPoints(FaceFrameResult faceResult)
		{
			bool isFaceValid = faceResult != null;

			if (isFaceValid)
			{
				var faceBox = faceResult.FaceBoundingBoxInColorSpace;
				if (faceBox != null)
				{
					// check if we have a valid rectangle within the bounds of the screen space
					isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
								  (faceBox.Bottom - faceBox.Top) > 0 &&
								  faceBox.Right <= this.displayWidth &&
								  faceBox.Bottom <= this.displayHeight;

					if (isFaceValid)
					{
						var facePoints = faceResult.FacePointsInColorSpace;
						if (facePoints != null)
						{
							foreach (PointF pointF in facePoints.Values)
							{
								// check if we have a valid face point within the bounds of the screen space
								bool isFacePointValid = pointF.X > 0.0f &&
														pointF.Y > 0.0f &&
														pointF.X < this.displayWidth &&
														pointF.Y < this.displayHeight;

								if (!isFacePointValid)
								{
									isFaceValid = false;
									break;
								}
							}
						}
					}
				}
			}

			return isFaceValid;
		}



		private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            draw = true;
           // previousPoint = e.GetPosition(this); //MouseControl.GetCursorPosition();
        }

		private void OnWaveForUserControl(object sender, EventArgs e)
		{
			draw = true;
		}

        private void drawArea_PointerMove(object sender, KinectPointerEventArgs args)
        {
			KinectPointerPoint kinectPointerPoint = args.CurrentPoint;			

			if (args.CurrentPoint.Properties.HandType == engagement.Draw() )
			{
				/*
				if (!drawArea.Children.Contains(handImg))
				{
					handImg.Source = handClipArt;
					handImg.Width = handClipArt.Width;
					handImg.Height = handClipArt.Height;
					drawArea.Children.Add(handImg);
				}
				*/
				Point currentPoint = new Point(kinectPointerPoint.Position.X * drawArea.ActualWidth, kinectPointerPoint.Position.Y * drawArea.ActualHeight); // MouseControl.GetCursorPosition();
				if (!kinectControl.penUp)
				{
					if (currentDrawingSegment == null)
					{
						currentDrawingSegment = new Polyline();
						currentDrawingSegment.Stroke = Brushes.MediumPurple;
						currentDrawingSegment.StrokeThickness = 3;
					}
					/*
					Canvas.SetLeft(handImg, previousPoint.X + handImg.Width / 2);
					Canvas.SetTop(handImg, previousPoint.Y + handImg.Height / 2);
					*/

					if (previousPoint != null)
					{
						currentDrawingSegment.Points.Add(currentPoint);

						Line line = new Line();
						line.Stroke = Brushes.DeepPink;
						line.X1 = previousPoint.X;
						line.X2 = currentPoint.X;
						line.Y1 = previousPoint.Y;
						line.Y2 = currentPoint.Y;
						line.StrokeThickness = 3;

						previousPoint = currentPoint;
						
						drawArea.Children.Add(line);
						
					}
				} else
				{
					previousPoint = currentPoint;
					if (currentDrawingSegment != null)
					{
						drawingSegments.Add(currentDrawingSegment);						
						DockPanel screenDisplay = userVideo;
						drawArea.Children.Clear();
						drawArea.Children.Add(screenDisplay);
						// drawArea.Children.Add(rect);
						Canvas.SetLeft(rect, 500);
						Canvas.SetTop(rect, 500);

						foreach (Polyline poly in drawingSegments)
						{
							drawArea.Children.Add(poly);
						}
						foreach (Point p in currentDrawingSegment.Points)
						{
							int drawingSegmentFace = this.PointOnFace(p);
							if (drawingSegmentFace != -1)
							{
								if (!drawingSegmentsFaces.ContainsKey(drawingSegmentFace))
								{
									drawingSegmentsFaces.Add(drawingSegmentFace, new List<Polyline>());
								}
								drawingSegmentsFaces[drawingSegmentFace].Add(currentDrawingSegment);

								// only take first face
								break;
							}
						}
						currentDrawingSegment = null;
					}
				}
				
			} 
			

        }

		private int PointOnFace(Point currentPoint)
		{
			for (int i = 0; i < bodyCount; i++)
			{
				if (faceFrameResults[i] != null)
				{
					var faceBox = faceFrameResults[i].FaceBoundingBoxInColorSpace;
					
					if (currentPoint.X > faceBox.Left && currentPoint.X < faceBox.Right && currentPoint.Y > faceBox.Top && currentPoint.Y < faceBox.Bottom)
					{
						return i;
					}
				}
			}
			return -1;
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

		private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
		{
			double x = rotQuaternion.X;
			double y = rotQuaternion.Y;
			double z = rotQuaternion.Z;
			double w = rotQuaternion.W;

			// convert face rotation quaternion to Euler angles in degrees
			double yawD, pitchD, rollD;
			pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
			yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
			rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

			// clamp the values to a multiple of the specified increment to control the refresh rate
			double increment = 5.0;
			pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
			yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
			roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
		}
	}


}
