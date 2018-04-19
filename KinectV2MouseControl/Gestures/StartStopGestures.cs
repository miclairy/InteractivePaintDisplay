using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;

namespace KinectV2InteractivePaint
{
	public class StartStopGestures
	{
		bool wasLeftGrip = false;
		/// <summary>
		/// If true, user did a right hand Grip gesture
		/// </summary>
		bool wasRightGrip = false;
		float handTraveledForwardAmount = 0;
		float handTraveledBackwardAmount = 0;


		private GestureController gestures = new GestureController();
		private HandType WaveDetected = HandType.NONE;
	//	public bool penUp = true;
		public Dictionary<ulong, bool> penUp = new Dictionary<ulong, bool>();
		private int stoppedTime;
		private Dictionary<ulong, CameraSpacePoint > prevHandLocationR = new Dictionary<ulong, CameraSpacePoint>();
		private Dictionary<ulong, CameraSpacePoint > prevHandLocationL = new Dictionary<ulong, CameraSpacePoint>();

		private int closedTimeoutR = 0;
		private int closedTimeoutL = 0;
		private Dictionary<ulong, CameraSpacePoint> prevHandLocationBackwardsL = new Dictionary<ulong, CameraSpacePoint>();
		private Dictionary<ulong, CameraSpacePoint> prevHandLocationForewardsL = new Dictionary<ulong, CameraSpacePoint>();
		private Dictionary<ulong, CameraSpacePoint> prevHandLocationBackwardsR = new Dictionary<ulong, CameraSpacePoint>();
		private Dictionary<ulong, CameraSpacePoint> prevHandLocationForewardsR = new Dictionary<ulong, CameraSpacePoint>();

		public StartStopGestures()
		{
		}

		public void DetectRightGestures(Body body, CameraSpacePoint handRight)
		{
			ulong id = body.TrackingId;
			if (!penUp.ContainsKey(body.TrackingId))
			{
				penUp.Add(body.TrackingId, true);
			}
			if (!prevHandLocationR.ContainsKey(id))
			{
				prevHandLocationR.Add(id, handRight);
				prevHandLocationBackwardsR.Add(id, handRight);
				prevHandLocationForewardsR.Add(id, handRight);

			}
			if (Math.Abs(handRight.Z - prevHandLocationR[id].Z) >= Math.Abs(handRight.X - prevHandLocationR[id].X) &&
				Math.Abs(handRight.Z - prevHandLocationR[id].Z) >= Math.Abs(handRight.Y - prevHandLocationR[id].Y))
			{
				if (handRight.Z < prevHandLocationR[id].Z) // moveing forward
				{
					if (handRight.Z + 0.2 < prevHandLocationForewardsR[id].Z)
					{
						penUp[body.TrackingId] = false;
						Console.WriteLine("Right start again draw");
					}
					prevHandLocationBackwardsR[id] = handRight;

				}
				if (handRight.Z > prevHandLocationR[id].Z) // moving backwards
				{

					if (handRight.Z > prevHandLocationBackwardsR[id].Z + 0.2)
					{
						penUp[body.TrackingId] = true;
						Console.WriteLine("Right hand stop draw");
					}

					prevHandLocationForewardsR[id] = handRight;
				}

			}
			prevHandLocationR[id] = handRight;
		}

		public void DetectLeftGestures(Body body, CameraSpacePoint handLeft)
		{
			ulong id = body.TrackingId;
			if (!penUp.ContainsKey(body.TrackingId))
			{
				penUp.Add(body.TrackingId, true);
			}
			if (!prevHandLocationL.ContainsKey(id))
			{
				prevHandLocationL.Add(id, handLeft);
				prevHandLocationForewardsL.Add(id, handLeft);
				prevHandLocationBackwardsL.Add(id, handLeft);
			}
			if (Math.Abs(handLeft.Z - prevHandLocationL[id].Z) >= Math.Abs(handLeft.X - prevHandLocationL[id].X) &&
				Math.Abs(handLeft.Z - prevHandLocationL[id].Z) >= Math.Abs(handLeft.Y - prevHandLocationL[id].Y))
			{
				if (handLeft.Z < prevHandLocationL[id].Z) // moving forward
				{
					if (handLeft.Z + 0.2 < prevHandLocationForewardsL[id].Z)
					{
						penUp[body.TrackingId] = false;
						Console.WriteLine("left start again draw");
					}
					prevHandLocationBackwardsL[id] = handLeft;

				}
				if (handLeft.Z > prevHandLocationL[id].Z) // moving backwards
				{

					if (handLeft.Z > prevHandLocationBackwardsL[id].Z + 0.2)
					{
						penUp[body.TrackingId] = true;
						Console.WriteLine("left hand stop draw");
					}

					prevHandLocationForewardsL[id] = handLeft;
				}

			}
			prevHandLocationL[id] = handLeft;
		}








		// old grip to take pen off 
	/*	public void DetectRightGripGestures(Body body, CameraSpacePoint handRight)
		{
			if (body.HandRightState != HandState.Closed)
			{

				if (handRight.Z < prevHandLocationR.Z)
				{
					if (handRight.Z + 0.1 < prevHandLocationR.Z)
					{
						penUp[body.TrackingId] = false;
						Console.WriteLine("right start again draw");
					}
				}
				else
				{
					closedTimeoutR += 1;
					prevHandLocationR = handRight;
				}
			}
			else if (closedTimeoutR < 100)
			{

				if (handRight.Z > prevHandLocationR.Z)
				{
					if (handRight.Z > prevHandLocationR.Z + 0.1)
					{
						penUp[body.TrackingId] = true;
						Console.WriteLine("right buffer closed hand stop draw");
					}
				}
				else
				{
					closedTimeoutR += 1;
					prevHandLocationR = handRight;
				}
			}
			else
			{
				closedTimeoutR = 0;
				if (handRight.Z > prevHandLocationR.Z)
				{
					if (handRight.Z > prevHandLocationR.Z + 0.1)
					{
						penUp[body.TrackingId] = true;
						Console.WriteLine("right closed hand stop draw");
					}
				}
				else
				{
					prevHandLocationR = handRight;
				}
			}

		}*/
	}

	

}