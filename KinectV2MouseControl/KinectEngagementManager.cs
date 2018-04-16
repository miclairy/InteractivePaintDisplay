using System;
using System.Collections.Generic;
using Microsoft.Kinect.Input;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Input;

namespace KinectV2InteractivePaint
{
	public class EngagementManager: IKinectEngagementManager
	{
		private KinectSensor sensor;
		private bool changed;
		private Body[] bodies;
		private BodyFrameReader bodyFrameReader;
		private GestureController gestures = new GestureController();
		private HandType WaveDetected;
		private ulong trackingId;


		public event EventHandler Engaged;
		public event EventHandler Disengaged;

		public EngagementManager(KinectSensor kinectSensor)
		{
			this.sensor = kinectSensor;
			DefineGestures();
			this.gestures.GestureRecognised += new EventHandler<GestureEventArgs>(this.Gestures_GestureRecognised);

		}

		public IReadOnlyList<BodyHandPair> KinectManualEngagedHands
		{
			get
			{
				return (KinectCoreWindow.KinectManualEngagedHands);
			}
		}

		public bool EngagedBodyHandPairsChanged()
		{
			return this.changed;
		}

		public void StartManaging()
		{
			this.bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
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
						if (this.bodies == null)
						{
							this.bodies = new Body[bodyFrame.BodyCount];
						}

						bodyFrame.GetAndRefreshBodyData(this.bodies);
					}
				}
			}
			var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;
			Console.WriteLine(currentlyEngagedHands.Count);

			foreach (Body body in this.bodies)
			{
			
				if (body != null && body.IsTracked)
				{
					this.gestures.UpdateAllGestures(body);
					if (this.WaveDetected != HandType.NONE)
					{
						this.EnsureEngaged(body.TrackingId);
					}
					break;
				}
			}
		}

		public HandType Draw()
		{
			return this.WaveDetected;
		}

		private void EnsureEngaged(ulong trackingId)
		{
			bool alreadyEngaged = false;
			
			if (this.trackingId != trackingId)
			{
				this.trackingId = trackingId;

				this.changed = true;

				KinectCoreWindow.SetKinectOnePersonManualEngagement(
				  new BodyHandPair(trackingId, this.WaveDetected));

				this.Engaged?.Invoke(this, EventArgs.Empty);
			} else
			{
				var currentlyEngagedHands = KinectCoreWindow.KinectManualEngagedHands;
				foreach (BodyHandPair pair in currentlyEngagedHands)
				{
					if (pair.BodyTrackingId == trackingId && pair.HandType != WaveDetected)
					{
						this.changed = true;

						KinectCoreWindow.SetKinectOnePersonManualEngagement(
						  new BodyHandPair(trackingId, this.WaveDetected));

						this.Engaged?.Invoke(this, EventArgs.Empty);
					}
				}
			}
		}

		private void DefineGestures()
		{

			IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
			WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
			WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
			waveLeftSegments[0] = waveLeftSegment1;
			waveLeftSegments[1] = waveLeftSegment2;
			waveLeftSegments[2] = waveLeftSegment1;
			waveLeftSegments[3] = waveLeftSegment2;
			waveLeftSegments[4] = waveLeftSegment1;
			waveLeftSegments[5] = waveLeftSegment2;
			this.gestures.AddGesture(GestureType.waveLeft, waveLeftSegments);

			IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[6];
			WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
			WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
			waveRightSegments[0] = waveRightSegment1;
			waveRightSegments[1] = waveRightSegment2;
			waveRightSegments[2] = waveRightSegment1;
			waveRightSegments[3] = waveRightSegment2;
			waveRightSegments[4] = waveRightSegment1;
			waveRightSegments[5] = waveRightSegment2;
			this.gestures.AddGesture(GestureType.waveRight, waveRightSegments);
		}

		private void Gestures_GestureRecognised(object sender, GestureEventArgs e)
		{
			if (e.type == GestureType.waveLeft)
			{
				this.WaveDetected = HandType.LEFT;
				Console.WriteLine("Detected left wave");

			}
			if (e.type == GestureType.waveRight)
			{
				this.WaveDetected = HandType.RIGHT;
				Console.WriteLine("Detected right wave");
				
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
