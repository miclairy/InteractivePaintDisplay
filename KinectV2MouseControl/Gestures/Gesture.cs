using System;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public class Gesture
	{
		private IRelativeGestureSegment[] gestureParts;
		private int currentGesturePart = 0;
		private int framePauseCount = 10; //pauses for ten frames when guesture is paused
		private int frameCount = 0;
		private bool paused = false;
		private GestureType type;

		public Gesture(GestureType type, IRelativeGestureSegment[] gestureParts)
		{
			this.type = type;
			this.gestureParts = gestureParts;
		}

		/// event for gesture
		public event EventHandler<GestureEventArgs> GestureRecognised;

		public void UpdateGesture(Body data)
		{
			if (this.paused)
			{
				if (this.frameCount >= this.framePauseCount)
				{
					this.paused = false;
				}
				this.frameCount++;
			}

			GestureResult result = this.gestureParts[this.currentGesturePart].CheckGesture(data);
			if (result == GestureResult.Suceed)
			{

				if (this.currentGesturePart + 1 < this.gestureParts.Length)
				{
					this.currentGesturePart++;
					this.ResetPause();
				} else
				{
					if (this.GestureRecognised != null)
					{
						this.GestureRecognised(this, new GestureEventArgs(this.gestureParts[this.currentGesturePart].GetGestureType(), data.TrackingId));
						this.Reset();
					}
				}
			} else if (result == GestureResult.Fail || this.frameCount == 50)
			{
				this.Reset();
			} else
			{
				this.frameCount = 0;
				this.framePauseCount = 5;
				this.paused = true;
			}
		}

		private void ResetPause()
		{
			this.frameCount = 0;
			this.framePauseCount = 10;
			this.paused = true;
		}

		public void Reset()
		{
			this.currentGesturePart = 0;
			this.frameCount = 0;
			this.framePauseCount = 5;
			this.paused = true;
		}
	}
}
