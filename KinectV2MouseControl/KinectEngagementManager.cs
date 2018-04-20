using System;
using System.Collections.Generic;
using Microsoft.Kinect.Input;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Input;

namespace KinectV2InteractivePaint
{
	public class EngagementManager
	{
		private KinectSensor sensor;
		private bool changed;
		private Body[] bodies;
		private BodyFrameReader bodyFrameReader;
		private GestureController gestures = new GestureController();
		private Dictionary<ulong, HandType> engagedHands = new Dictionary<ulong, HandType>();
		// private ulong trackingId;
		private CoordinateMapper coordinateMapper;
		public List<ulong> engagedBodies = new List<ulong>();

		public event EventHandler Engaged;
		public event EventHandler Disengaged;

		public EngagementManager(KinectSensor kinectSensor)
		{
			sensor = kinectSensor;
			DefineGestures();
			gestures.GestureRecognised += new EventHandler<GestureEventArgs>(this.Gestures_GestureRecognised);
			coordinateMapper = kinectSensor.CoordinateMapper;
			StartManaging();
		}

		/*public IReadOnlyList<BodyHandPair> KinectManualEngagedHands
		{
			get
			{
				return (KinectCoreWindow.KinectManualEngagedHands);
			}
		}*/

		public bool EngagedBodyHandPairsChanged()
		{
			return this.changed;
		}

		public void StartManaging()
		{
			bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
			bodyFrameReader = sensor.BodyFrameSource.OpenReader();
			bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
		}

		private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{
			if (e.FrameReference != null)
			{
				using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
				{
					if (bodyFrame != null)
					{
						if (bodies == null)
						{
							bodies = new Body[bodyFrame.BodyCount];
						}

						bodyFrame.GetAndRefreshBodyData(bodies);
					}
				}
			}
		//	var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;

			foreach (Body body in bodies)
			{
			
				if (body != null && body.IsTracked)
				{
					gestures.UpdateAllGestures(body);

					if (engagedHands.ContainsKey(body.TrackingId) && engagedHands[body.TrackingId] != HandType.NONE)
					{
						EnsureEngaged(body.TrackingId);
						
					}

					
				}
			}
		}

		public HandType Draw(ulong TrackingId)
		{
			if (engagedHands.ContainsKey(TrackingId))
			{
				return engagedHands[TrackingId];
			}
			return HandType.NONE;
		}

		private void EnsureEngaged(ulong trackingId)
		{
			
			if (!engagedBodies.Contains(trackingId))
			{
				// this.trackingId = trackingId;
				engagedBodies.Add(trackingId);

				changed = true;

				//KinectCoreWindow.SetKinectOnePersonManualEngagement(
				//  new BodyHandPair(trackingId, this.WaveDetected));
				

			//	Engaged?.Invoke(this, EventArgs.Empty);

			} else
			{
				//var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;
				/*foreach (KeyValuePair<ulong, HandType> pair in engagedHands)
				{
					if (pair.Key == trackingId && pair.Value != WaveDetected)
					{
						changed = true;*/

						// KinectCoreWindow.SetKinectOnePersonManualEngagement(
						  // new BodyHandPair(trackingId, this.WaveDetected));

						// Engaged?.Invoke(this, EventArgs.Empty);
				//	}
				// }
			}
		}

		private void DefineGestures()
		{

			IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[4];
			WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
			WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
			waveLeftSegments[0] = waveLeftSegment1;
			waveLeftSegments[1] = waveLeftSegment2;
			waveLeftSegments[2] = waveLeftSegment1;
			waveLeftSegments[3] = waveLeftSegment2;

			this.gestures.AddGesture(GestureType.waveLeft, waveLeftSegments);

			IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[4];
			WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
			WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
			waveRightSegments[0] = waveRightSegment1;
			waveRightSegments[1] = waveRightSegment2;
			waveRightSegments[2] = waveRightSegment1;
			waveRightSegments[3] = waveRightSegment2;

			this.gestures.AddGesture(GestureType.waveRight, waveRightSegments);

			IRelativeGestureSegment[] dropGestureSegmentsLeft = new IRelativeGestureSegment[2];
			StopDrawingSegmentLeft dropSegmentLeft = new StopDrawingSegmentLeft();
			dropGestureSegmentsLeft[0] = dropSegmentLeft;
			dropGestureSegmentsLeft[1] = dropSegmentLeft;
			this.gestures.AddGesture(GestureType.stopDrawing, dropGestureSegmentsLeft);

			IRelativeGestureSegment[] dropGestureSegmentsRight = new IRelativeGestureSegment[2];
			StopDrawingSegmentRight dropSegmentRight = new StopDrawingSegmentRight();
			dropGestureSegmentsRight[0] = dropSegmentRight;
			dropGestureSegmentsRight[1] = dropSegmentRight;
			this.gestures.AddGesture(GestureType.stopDrawing, dropGestureSegmentsRight);
		}

		private void Gestures_GestureRecognised(object sender, GestureEventArgs e)
		{
			if (e.type == GestureType.waveLeft)
			{
				if (engagedHands.ContainsKey(e.trackingId))
				{
					engagedHands[e.trackingId] = HandType.LEFT;
				}
				else
				{
					engagedHands.Add(e.trackingId, HandType.LEFT);
				}
				Console.WriteLine("Detected left wave");

			}
			if (e.type == GestureType.waveRight)
			{
				if (engagedHands.ContainsKey(e.trackingId))
				{
					engagedHands[e.trackingId] = HandType.RIGHT;
				}
				else
				{
					engagedHands.Add(e.trackingId, HandType.RIGHT);
				}

				Console.WriteLine("Detected right wave");
				
			}
			if (e.type == GestureType.stopDrawing)
			{
				if (engagedHands.ContainsKey(e.trackingId))
				{
					engagedHands.Remove(e.trackingId);
				}
				Console.WriteLine("Detected drop");
			}
		}

		public void StopManaging()
		{
			this.bodyFrameReader.FrameArrived -= bodyFrameReader_FrameArrived;
			this.bodyFrameReader.Dispose();
			this.bodies = null;
		}
	}
}
