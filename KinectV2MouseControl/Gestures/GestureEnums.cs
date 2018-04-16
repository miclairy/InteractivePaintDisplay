using System;

namespace KinectV2InteractivePaint
{
	public enum GestureResult
	{
		Suceed, Pausing, Fail
	}

	public enum GestureType
	{
		waveLeft, waveRight
	}

	public class GestureEventArgs: EventArgs
	{
		public GestureType type;
		public ulong trackingId;

		public GestureEventArgs(GestureType type, ulong trackingID)
		{
			this.type = type;
			this.trackingId = trackingID;
		}
	}
}
