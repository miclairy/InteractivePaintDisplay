using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Kinect;
using Microsoft.Kinect.Input;

namespace KinectV2InteractivePaint
{
	public class ColourGestures
	{
		public ColourGestures()
		{
		}

		public static double DetectColourGestures(CameraSpacePoint engagedHand, CameraSpacePoint otherHand)
		{
			if (Math.Abs(engagedHand.X - otherHand.X) <= 0.2 && Math.Abs(engagedHand.Y - otherHand.Y) <= 0.2)
			{
				double angle = Math.Atan2(otherHand.X - engagedHand.X, otherHand.Y - engagedHand.Y);
				if (angle < 0.0)
				{
					angle += Math.PI * 2;
				}
			//	Console.WriteLine("hands close " + angle * (180.0 / Math.PI));
				angle = angle * (180.0 / Math.PI);
				return angle;
			}
			else return -1;
		}


		public static Color HsvToRgb(double h, double S, double V)
		{
			double H = h;
			while (H < 0) { H += 360; };
			while (H >= 360) { H -= 360; };
			double R, G, B;
			if (V <= 0)
			{ R = G = B = 0; }
			else if (S <= 0)
			{
				R = G = B = V;
			}
			else
			{
				double hf = H / 60.0;
				int i = (int)Math.Floor(hf);
				double f = hf - i;
				double pv = V * (1 - S);
				double qv = V * (1 - S * f);
				double tv = V * (1 - S * (1 - f));
				switch (i)
				{

					// Red is the dominant color

					case 0:
						R = V;
						G = tv;
						B = pv;
						break;

					// Green is the dominant color

					case 1:
						R = qv;
						G = V;
						B = pv;
						break;
					case 2:
						R = pv;
						G = V;
						B = tv;
						break;

					// Blue is the dominant color

					case 3:
						R = pv;
						G = qv;
						B = V;
						break;
					case 4:
						R = tv;
						G = pv;
						B = V;
						break;

					// Red is the dominant color

					case 5:
						R = V;
						G = pv;
						B = qv;
						break;

					// Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

					case 6:
						R = V;
						G = tv;
						B = pv;
						break;
					case -1:
						R = V;
						G = pv;
						B = qv;
						break;

					// The color is not defined, we should throw an error.

					default:
						//LFATAL("i Value error in Pixel conversion, Value is %d", i);
						R = G = B = V; // Just pretend its black/white
						break;
				}
			}
			int r = Clamp((int)(R * 255.0));
			int g = Clamp((int)(G * 255.0));
			int b = Clamp((int)(B * 255.0));

		//	Console.WriteLine(h + " =  " + r + " " + b + " " + g);
			
			return Color.FromRgb(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
		}

		/// <summary>
		/// Clamp a value to 0-255
		/// </summary>
		static int Clamp(int i)
		{
			if (i < 0) return 0;
			if (i > 255) return 255;
			return i;
		}
	}


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

		public void DetectStopGesture(Body body, CameraSpacePoint engagedHand, CameraSpacePoint otherHand)
		{
			if (Math.Abs(engagedHand.X - otherHand.X) <= 0.2 && Math.Abs(engagedHand.Y - otherHand.Y) <= 0.2)
			{
				penUp[body.TrackingId] = true;
			}
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