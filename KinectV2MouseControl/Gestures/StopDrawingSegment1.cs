using System;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public class StopDrawingSegment1: IRelativeGestureSegment
	{
		public StopDrawingSegment1()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			if (skeleton.HandLeftState == HandState.Closed )
			{
				return GestureResult.Suceed;
			}
			return GestureResult.Fail;
			
		}

		public GestureType GetGestureType()
		{
			return GestureType.stopDrawing;
		}
	}

	public class StopDrawingSegment2 : IRelativeGestureSegment
	{
		public StopDrawingSegment2()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			if (skeleton.HandLeftState == HandState.Closed)
			{
				if (skeleton.Joints[JointType.HandLeft].Position.Z > skeleton.Joints[JointType.ElbowLeft].Position.Z) { 

					return GestureResult.Suceed;
				}
				return GestureResult.Pausing;
			}
			return GestureResult.Fail;

		}

		public GestureType GetGestureType()
		{
			return GestureType.stopDrawing;
		}
	}
}
