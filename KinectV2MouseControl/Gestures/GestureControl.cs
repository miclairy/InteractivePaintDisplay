using System;
using System.Collections.Generic;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{
	public class GestureController
	{

		private List<Gesture> gestures = new List<Gesture>();

		public GestureController()
		{

		}

		public event EventHandler<GestureEventArgs> GestureRecognised;

		public void UpdateAllGestures(Body data)
		{
			foreach (Gesture gesture in this.gestures)
			{
				gesture.UpdateGesture(data);
			}
		}

		public void AddGesture(GestureType type, IRelativeGestureSegment[] gestureDefinition)
		{
			Gesture gesture = new Gesture(type, gestureDefinition);
			gesture.GestureRecognised += new EventHandler<GestureEventArgs>(this.Gesture_GestureRecognised);
			this.gestures.Add(gesture);

		}

		private void Gesture_GestureRecognised(Object sender, GestureEventArgs e)
		{

			if (this.GestureRecognised != null)
			{
				this.GestureRecognised(this, e);
			}

			foreach (Gesture gesture in this.gestures)
			{
				gesture.Reset();
			}
		}
		
	}

}