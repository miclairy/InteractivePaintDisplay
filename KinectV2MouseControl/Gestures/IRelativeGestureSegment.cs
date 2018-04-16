using System;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public interface IRelativeGestureSegment
	{

		GestureType GetGestureType();

		GestureResult CheckGesture(Body skeleton);
	}

}
