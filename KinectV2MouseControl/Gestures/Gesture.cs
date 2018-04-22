using System;
using System.Collections.Generic;
using Microsoft.Kinect;

namespace KinectV2InteractivePaint
{

	public class Gesture
	{
		private IRelativeGestureSegment[] gestureParts;
		private Dictionary<ulong, int> currentGesturePart = new Dictionary<ulong, int>();
		private Dictionary<ulong, int> framePauseCount = new Dictionary<ulong, int>(); //pauses for ten frames when guesture is paused
		private Dictionary<ulong, int> frameCount = new Dictionary<ulong, int>();
		private Dictionary<ulong, bool> paused = new Dictionary<ulong, bool>();
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
			ulong id = data.TrackingId;
			if (!currentGesturePart.ContainsKey(data.TrackingId))
			{
				currentGesturePart.Add(data.TrackingId, 0);
			}
			if (!frameCount.ContainsKey(id)) {
				frameCount.Add(data.TrackingId, 0);
			}
			if (!framePauseCount.ContainsKey(id)) {
				framePauseCount.Add(data.TrackingId, 10);
			}
			if (!paused.ContainsKey(id)) {
				paused.Add(data.TrackingId, false);
			}
			
			if (this.paused[id])
			{
				if (this.frameCount[id] >= this.framePauseCount[id])
				{
					this.paused[id] = false;
				}
				this.frameCount[id]++;
			}

			GestureResult result = this.gestureParts[this.currentGesturePart[data.TrackingId]].CheckGesture(data);
			if (result == GestureResult.Suceed)
			{

				if (this.currentGesturePart[data.TrackingId] + 1 < this.gestureParts.Length)
				{
					this.currentGesturePart[data.TrackingId] ++;
					this.ResetPause(id);
				} else
				{
					if (this.GestureRecognised != null)
					{
						this.GestureRecognised(this, new GestureEventArgs(this.gestureParts[this.currentGesturePart[data.TrackingId]].GetGestureType(), data.TrackingId));
						this.Reset(id);
					}
				}
			} else if (result == GestureResult.Fail || this.frameCount[id] == 80)
			{
				this.Reset(id);
			} else
			{
				this.frameCount[id] = 0;
				this.framePauseCount[id] = 5;
				this.paused[id] = true;
			}
		}

		private void ResetPause(ulong id)
		{
			this.frameCount[id] = 0;
			this.framePauseCount[id] = 10;
			this.paused[id] = true;
		}

		public void Reset(ulong id)
		{
			this.currentGesturePart = new Dictionary<ulong, int>();
			this.frameCount[id] = 0;
			this.framePauseCount[id] = 5;
			this.paused[id] = true;
		}
	}
}
