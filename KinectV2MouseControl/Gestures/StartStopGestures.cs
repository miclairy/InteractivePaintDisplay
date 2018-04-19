using System;
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
		public bool penUp = true;
		private int stoppedTime;
		private CameraSpacePoint prevHandLocationR;
		private CameraSpacePoint prevHandLocationL;

		private int closedTimeoutR = 0;
		private int closedTimeoutL = 0;
		private CameraSpacePoint prevHandLocationBackwardsL;
		private CameraSpacePoint prevHandLocationForewardsL;
		private CameraSpacePoint prevHandLocationBackwardsR;
		private CameraSpacePoint prevHandLocationForewardsR;

		public StartStopGestures()
		{
		}

		public void DetectRightGestures(Body body, CameraSpacePoint handRight)
		{
			if (Math.Abs(handRight.Z - prevHandLocationR.Z) >= Math.Abs(handRight.X - prevHandLocationR.X) &&
				Math.Abs(handRight.Z - prevHandLocationR.Z) >= Math.Abs(handRight.Y - prevHandLocationR.Y))
			{
				if (handRight.Z < prevHandLocationR.Z) // moveing forward
				{
					if (handRight.Z + 0.2 < prevHandLocationForewardsR.Z)
					{
						penUp = false;
						Console.WriteLine("Right start again draw");
					}
					prevHandLocationBackwardsR = handRight;

				}
				if (handRight.Z > prevHandLocationR.Z) // moving backwards
				{

					if (handRight.Z > prevHandLocationBackwardsR.Z + 0.2)
					{
						penUp = true;
						Console.WriteLine("Right hand stop draw");
					}

					prevHandLocationForewardsR = handRight;
				}

			}
			prevHandLocationR = handRight;
		}

		public void DetectLeftGestures(Body body, CameraSpacePoint handLeft)
		{
			if (Math.Abs(handLeft.Z - prevHandLocationL.Z) >= Math.Abs(handLeft.X - prevHandLocationL.X) &&
				Math.Abs(handLeft.Z - prevHandLocationL.Z) >= Math.Abs(handLeft.Y - prevHandLocationL.Y))
			{
				if (handLeft.Z < prevHandLocationL.Z) // moveing forward
				{
					if (handLeft.Z + 0.2 < prevHandLocationForewardsL.Z)
					{
						penUp = false;
						Console.WriteLine("left start again draw");
					}
					prevHandLocationBackwardsL = handLeft;

				}
				if (handLeft.Z > prevHandLocationL.Z) // moving backwards
				{

					if (handLeft.Z > prevHandLocationBackwardsL.Z + 0.2)
					{
						penUp = true;
						Console.WriteLine("left hand stop draw");
					}

					prevHandLocationForewardsL = handLeft;
				}

			}
			prevHandLocationL = handLeft;
		}

		// old grip to take pen off 
		public void DetectRightGripGestures(Body body, CameraSpacePoint handRight)
		{
			if (body.HandRightState != HandState.Closed)
			{

				if (handRight.Z < prevHandLocationR.Z)
				{
					if (handRight.Z + 0.1 < prevHandLocationR.Z)
					{
						penUp = false;
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
						penUp = true;
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
						penUp = true;
						Console.WriteLine("right closed hand stop draw");
					}
				}
				else
				{
					prevHandLocationR = handRight;
				}
			}

		}
	}

	

}