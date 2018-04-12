using System;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{

	public interface IRelativeGestureSegment
	{

		GestureType GetGestureType();

		GestureResult CheckGesture(Body skeleton);
	}

}
