using System;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public class StopDrawingSegmentLeft: IRelativeGestureSegment
	{
		public StopDrawingSegmentLeft()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			double handY = skeleton.Joints[JointType.HandLeft].Position.Y;
			double elbowY = skeleton.Joints[JointType.ElbowLeft].Position.Y;
			double handZ = skeleton.Joints[JointType.HandLeft].Position.Z;
			double elbowZ = skeleton.Joints[JointType.ElbowLeft].Position.Z;

			double angle = Math.Atan2(handZ - elbowZ, handY - elbowY);
			angle = angle + Math.PI * 2;
			// Console.WriteLine(handY + " " + elbowY  +" "+ handZ + " " + elbowZ + " "+ angle);
			if (handY + 0.2 < elbowY)
			{
				return GestureResult.Suceed;
			}
			return GestureResult.Fail;
			
		}

		public GestureType GetGestureType()
		{
			return GestureType.stopDrawingLeft;
		}
	}

	public class StopDrawingSegmentRight : IRelativeGestureSegment
	{
		public StopDrawingSegmentRight()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			double handY = skeleton.Joints[JointType.HandRight].Position.Y;
			double elbowY = skeleton.Joints[JointType.ElbowRight].Position.Y;
			double handZ = skeleton.Joints[JointType.HandRight].Position.Z;
			double elbowZ = skeleton.Joints[JointType.ElbowRight].Position.Z;

			double angle = Math.Atan2(handZ - elbowZ, handY - elbowZ);
			// Console.WriteLine(handY + " " + elbowY  +" "+ handZ + " " + elbowZ + " "+ angle);

			if (handY + 0.2 < elbowY)
			{
				return GestureResult.Suceed;
			}
			return GestureResult.Fail;

		}

		public GestureType GetGestureType()
		{
			return GestureType.stopDrawingRight;
		}
	}


}
