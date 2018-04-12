using System;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{

	public class WaveRightSegment1 : IRelativeGestureSegment
	{

		public WaveRightSegment1()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			// Right hand above elbow
			if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
			{
				// Right hand right of elbow
				if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ElbowRight].Position.X)
				{

					return GestureResult.Suceed;
				}

				return GestureResult.Pausing;
			}

			return GestureResult.Fail;
		}

		public GestureType GetGestureType()
		{
			return GestureType.waveRight;
		}
	}


	public class WaveRightSegment2 : IRelativeGestureSegment
	{
		public WaveRightSegment2()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{

			// Right hand above elbow
			if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
			{
				// Right hand Right of elbow
				if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X)
				{

					return GestureResult.Suceed;
				}

				return GestureResult.Pausing;
			}

			return GestureResult.Fail;
		}

		public GestureType GetGestureType()
		{
			return GestureType.waveRight;
		}
	}

	

}
