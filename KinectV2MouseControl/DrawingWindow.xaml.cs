using System.Windows;
using System;
using System.Windows.Input;
using System.Windows.Shapes;

namespace KinectV2MouseControl
{
    public partial class DrawingWindow: Window
    {
        MouseControl mouseControl = new MouseControl();
        private Point previousPoint;
        private bool draw = false;
       

        public DrawingWindow()
        {
            InitializeComponent();
            previousPoint = MouseControl.GetCursorPosition();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            draw = true;
            previousPoint = e.GetPosition(this); //MouseControl.GetCursorPosition();
        }

        private void drawArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (draw)
            {
                Point currentPoint = e.GetPosition(this);// MouseControl.GetCursorPosition();
           
                Line line = new Line();
                line.Stroke = System.Windows.Media.Brushes.DeepPink;
                line.X1 = previousPoint.X;
                line.X2 = currentPoint.X;
                line.Y1 = previousPoint.Y;
                line.Y2 = currentPoint.Y;
                line.StrokeThickness = 3;
                previousPoint = currentPoint;

                drawArea.Children.Add(line);
            }

        }

        private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            draw = false;
        }
    }


}
