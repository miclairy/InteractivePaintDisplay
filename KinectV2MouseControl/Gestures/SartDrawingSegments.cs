using System;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public class StartDrawingSegment1: IRelativeGestureSegment
	{
		public StartDrawingSegment1()
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
			return GestureType.startDrawing;
		}
	}

	public class StartDrawingSegment2 : IRelativeGestureSegment
	{
		public StartDrawingSegment2()
		{
		}

		public GestureResult CheckGesture(Body skeleton)
		{
			if (skeleton.HandLeftState != HandState.Closed)
			{
				if (skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ElbowLeft].Position.Z) { 

					return GestureResult.Suceed;
				}
				return GestureResult.Pausing;
			}
			return GestureResult.Fail;

		}

		public GestureType GetGestureType()
		{
			return GestureType.startDrawing;
		}
	}
}
