using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Kinect;

namespace KinectV2MouseControl
{
    class KinectControl
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        KinectSensor sensor;
        /// <summary>
        /// Reader for body frames
        /// </summary>
        BodyFrameReader bodyFrameReader;
        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;
        /// <summary>
        /// Screen width and height for determining the exact mouse sensitivity
        /// </summary>
        int screenWidth, screenHeight;

        /// <summary>
        /// timer for pause-to-click feature
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// How far the cursor move according to your hand's movement
        /// </summary>
        public float mouseSensitivity = MOUSE_SENSITIVITY;

        /// <summary>
        /// Time required as a pause-clicking
        /// </summary>
        public float timeRequired = TIME_REQUIRED;
        /// <summary>
        /// The radius range your hand move inside a circle for [timeRequired] seconds would be regarded as a pause-clicking
        /// </summary>
        public float pauseThresold = PAUSE_THRESOLD;
        /// <summary>
        /// Decide if the user need to do clicks or only move the cursor
        /// </summary>
        public bool doClick = DO_CLICK;
        /// <summary>
        /// Use Grip gesture to click or not
        /// </summary>
        public bool useGripGesture = USE_GRIP_GESTURE;
        /// <summary>
        /// Value 0 - 0.95f, the larger it is, the smoother the cursor would move
        /// </summary>
        public float cursorSmoothing = CURSOR_SMOOTHING;

        // Default values
        public const float MOUSE_SENSITIVITY = 3.5f;
        public const float TIME_REQUIRED = 2f;
        public const float PAUSE_THRESOLD = 60f;
        public const bool DO_CLICK = true;
        public const bool USE_GRIP_GESTURE = true;
        public const float CURSOR_SMOOTHING = 0.2f;

        /// <summary>
        /// Determine if we have tracked the hand and used it to move the cursor,
        /// If false, meaning the user may not lift their hands, we don't get the last hand position and some actions like pause-to-click won't be executed.
        /// </summary>
        bool alreadyTrackedPos = false;

        /// <summary>
        /// for storing the time passed for pause-to-click
        /// </summary>
        float timeCount = 0;
        /// <summary>
        /// For storing last cursor position
        /// </summary>
        Point lastCurPos = new Point(0, 0);

        /// <summary>
        /// If true, user did a left hand Grip gesture
        /// </summary>
        bool wasLeftGrip = false;
        /// <summary>
        /// If true, user did a right hand Grip gesture
        /// </summary>
        bool wasRightGrip = false;

		private GestureController gestures = new GestureController();
		private int WaveDetected = -1;  // 1 = left hand wave, 2 = right hand wave

		

		public KinectControl()
        {
            // get Active Kinect Sensor
            sensor = KinectSensor.GetDefault();
            // open the reader for the body frames
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // get screen with and height
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // set up timer, execute every 0.1s
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100); 
　　　　    timer.Tick += new EventHandler(Timer_Tick);
　　　　    timer.Start();

			this.DefineGestures();
            this.gestures.GestureRecognised += new EventHandler<GestureEventArgs>(this.Gestures_GestureRecognised);

			// open the sensor
			sensor.Open();
        }


        
        /// <summary>
        /// Pause to click timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Tick(object sender, EventArgs e)
        {
            if (!doClick || useGripGesture) return;

            if (!alreadyTrackedPos) {
                timeCount = 0;
                return;
            }
            
            Point curPos = MouseControl.GetCursorPosition();

            if ((lastCurPos - curPos).Length < pauseThresold)
            {
                if ((timeCount += 0.1f) > timeRequired)
                {
                    //MouseControl.MouseLeftDown();
                    //MouseControl.MouseLeftUp();
                    MouseControl.DoMouseClick();
                    timeCount = 0;
                }
            }
            else
            {
                timeCount = 0;
            }

            lastCurPos = curPos;
        }

        /// <summary>
        /// Read body frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived) 
            {
                alreadyTrackedPos = false;
                return;
            }

            foreach (Body body in this.bodies)
            {

                // get first tracked body only, notice there's a break below.
                if (body.IsTracked)
                {
					//recognise user defined gestures
					this.gestures.UpdateAllGestures(body);

                    // get various skeletal positions
                    CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                    CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;

                    if (this.WaveDetected == 2) // if right hand waved
                    {
                        /* hand x calculated by this. don't use shoulder right as a reference cause the shoulder right
                         * is usually behind the lift right hand, and the position would be inferred and unstable.
                         * because the spine base is on the left of right hand, we plus 0.20f to make it closer to the right. */
                        float x = handRight.X - spineBase.X + 0.20f;
                        /* hand y calculated by this. ss spine base is way lower than right hand, plus 0.50f to make it
                         * higer */
                        float y = spineBase.Y - handRight.Y + 0.50f;
                        // get current cursor position
                        Point curPos = MouseControl.GetCursorPosition();
                        // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
                        float smoothing = 1 - cursorSmoothing;
						// set cursor position
						MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));

                        alreadyTrackedPos = true;

						

                        // Grip gesture
                        if (doClick && useGripGesture)
                        {
                            if (body.HandRightState == HandState.Closed)
                            {
                                if (!wasRightGrip)
                                {
                                    MouseControl.MouseLeftDown();
                                    wasRightGrip = true;
                                }
                            }
                            else if (body.HandRightState == HandState.Open)
                            {
                                if (wasRightGrip)
                                {
                                    MouseControl.MouseLeftUp();
                                    wasRightGrip = false;
                                }
                            }
                        }
                    }
                    else if (this.WaveDetected == 1) // if left hand waved
                    {
                        float x = handLeft.X - spineBase.X + 0.2f;
                        float y = spineBase.Y - handLeft.Y + 0.5f;
                        Point curPos = MouseControl.GetCursorPosition();
                        float smoothing = 1 - cursorSmoothing;
						MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));
                        alreadyTrackedPos = true;

                        if (doClick && useGripGesture)
                        {
                            if (body.HandLeftState == HandState.Closed)
                            {
                                if (!wasLeftGrip)
                                {
                                    MouseControl.MouseLeftDown();
                                    wasLeftGrip = true;
                                }
                            }
                            else if (body.HandLeftState == HandState.Open)
                            {
                                if (wasLeftGrip)
                                {
                                    MouseControl.MouseLeftUp();
                                    wasLeftGrip = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        wasLeftGrip = true;
                        wasRightGrip = true;
                        alreadyTrackedPos = false;
                    }

                    // get first tracked body only
                    break;
                }
            }
        }

		public bool Draw()
		{
			return !(this.WaveDetected == -1);
		}


		private void Gestures_GestureRecognised(object sender, GestureEventArgs e)
		{
			if (e.type == GestureType.waveLeft)
			{
				this.WaveDetected = 1;
				Console.WriteLine("Detected left wave");
				
			}
			if (e.type == GestureType.waveRight)
			{
				this.WaveDetected = 2;
				Console.WriteLine("Detected right wave");
			}
		}

		private void DefineGestures()
		{

			IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
			WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
			WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
			waveLeftSegments[0] = waveLeftSegment1;
			waveLeftSegments[1] = waveLeftSegment2;
			waveLeftSegments[2] = waveLeftSegment1;
			waveLeftSegments[3] = waveLeftSegment2;
			waveLeftSegments[4] = waveLeftSegment1;
			waveLeftSegments[5] = waveLeftSegment2;
			this.gestures.AddGesture(GestureType.waveLeft, waveLeftSegments);

			IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[6];
			WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
			WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
			waveRightSegments[0] = waveRightSegment1;
			waveRightSegments[1] = waveRightSegment2;
			waveRightSegments[2] = waveRightSegment1;
			waveRightSegments[3] = waveRightSegment2;
			waveRightSegments[4] = waveRightSegment1;
			waveRightSegments[5] = waveRightSegment2;
			this.gestures.AddGesture(GestureType.waveRight, waveRightSegments);
		}

        public void Close()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }

    }
}
