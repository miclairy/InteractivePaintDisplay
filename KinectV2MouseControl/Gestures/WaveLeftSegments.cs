using System;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{

	public class WaveLeftSegment1 : IRelativeGestureSegment
	{

		public WaveLeftSegment1()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			// left hand above elbow
			if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
			{
				// left hand right of elbow
				if (skeleton.Joints[JointType.HandLeft].Position.X > skeleton.Joints[JointType.ElbowLeft].Position.X)
				{

					return GestureResult.Suceed;
				}

				return GestureResult.Pausing;
			}

			return GestureResult.Fail;
		}

		public GestureType GetGestureType()
		{
			return GestureType.waveLeft;
		}
	}


	public class WaveLeftSegment2 : IRelativeGestureSegment
	{
		public WaveLeftSegment2()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{

			// left hand above elbow
			if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.ElbowLeft].Position.Y)
			{
				// left hand left of elbow
				if (skeleton.Joints[JointType.HandLeft].Position.X < skeleton.Joints[JointType.ElbowLeft].Position.X)
				{

					return GestureResult.Suceed;
				}

				return GestureResult.Pausing;
			}

			return GestureResult.Fail;
		}

		public GestureType GetGestureType()
		{
			return GestureType.waveLeft;
		}
	}

	

}
