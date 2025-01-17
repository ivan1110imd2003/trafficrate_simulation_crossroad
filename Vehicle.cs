﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace UrbanRate
{

    abstract class Vehicle
    {
        protected PictureBox img;
        protected Form1 mainForm;
        protected List<Point> trajectory;
        protected double velocity;
        protected int index;
        protected int position; // 1 - top; 2 - right; 3 - left; 4 - bottom 
        protected Boolean waitedOnTrafficLight = false;

        int t;
        System.Timers.Timer T;
        System.Timers.Timer LightWait;

        public void Start()
        {
            index = 1;
            t = 0;
            T = new System.Timers.Timer(50);
            T.Elapsed += OnMove;
            T.Enabled = true;
            if (this is Car)
            {
	            if (position == 1) mainForm.topCount++;
	            else if (position == 2) mainForm.rightCount++;
	            else if (position == 3) mainForm.leftCount++;
	            else if (position == 4) mainForm.bottomCount++;
            }
        }

        public void Stop()
        {
            T.Stop();
            T.Enabled = false;
            img.BeginInvoke((MethodInvoker)delegate () { img.Visible = false; });
            if (waitedOnTrafficLight == false)
            {
	            if (position == 1) mainForm.topCount--;
	            else if (position == 2) mainForm.rightCount--;
	            else if (position == 3) mainForm.leftCount--;
	            else if (position == 4) mainForm.bottomCount--;
            }
            if (this is Ambulance || velocity >= 35)
            {
            	if (mainForm.bottomState == false) mainForm.barrierBottom.BackColor = Color.FromArgb(255, 128, 128);
            	if (mainForm.topState == false) mainForm.barrierTop.BackColor = Color.FromArgb(255, 128, 128);
            	if (mainForm.leftState == false) mainForm.barrierLeft.BackColor = Color.FromArgb(255, 128, 128);
            	if (mainForm.rightState == false) mainForm.barrierRight.BackColor = Color.FromArgb(255, 128, 128);
            }
            if (velocity >= 35)
            {
            	if (position == 1) mainForm.topFastCar= false;
	            else if (position == 2) mainForm.rightFastCar = false;
	            else if (position == 3) mainForm.leftFastCar = false;
	            else if (position == 4) mainForm.bottomFastCar = false;
            }
        }

        void OnMove(Object sender, ElapsedEventArgs e)
        {
            t++;
            int x1 = trajectory[index].X - trajectory[index - 1].X, y1 = trajectory[index].Y - trajectory[index - 1].Y;
            double L = Math.Sqrt(x1 * x1 + y1 * y1);
            if (t > L / velocity)
            {
                index++;
                if (index == trajectory.Count)
                {
                    Stop(); return;
                }
                t = 0;
                x1 = trajectory[index].X - trajectory[index - 1].X; y1 = trajectory[index].Y - trajectory[index - 1].Y;
                int x2 = 1, y2 = 0;
                double angle = Math.Atan2(x2 * y1 - y2 * x1, x1 * x2 + y1 * y2);
                img.BeginInvoke((MethodInvoker)delegate { 
                    if (this is Ambulance) img.Image = ImageTools.RotateImage((Bitmap)Properties.Resources.AmbulanceImage, (float)(180.0 * angle / Math.PI));
                    else img.Image = ImageTools.RotateImage((Bitmap)Properties.Resources.CarImage, (float)(180.0 * angle / Math.PI)); 
                });
            }
            else
            {
                double L0 = velocity * t;
                double x = trajectory[index - 1].X + L0 * x1 / L, y = trajectory[index - 1].Y + L0 * y1 / L;
                img.BeginInvoke((MethodInvoker)delegate () { img.Location = new Point((int)x, (int)y); });
                //if (this is Ambulance) return;
                if (position == 3 || position == 2) CheckTrafficLight(Convert.ToInt32(x));
                else CheckTrafficLight(Convert.ToInt32(y));
            }
        }
        
        public void CheckTrafficLight(int currentXY)
        {
        	int timerInterval = 1000;
        	if (position == 3 && mainForm.leftCount == 2) { currentXY+=160; timerInterval=3000; }
        	else if (position == 2 && mainForm.rightCount == 2) { currentXY-=160; timerInterval=3000; }
        	else if (position == 1 && mainForm.topCount == 2) { currentXY+=160; timerInterval=2000; }
        	else if (position == 4 && mainForm.bottomCount == 2) { currentXY-=160; timerInterval=2000; } // 1 - top; 2 - right; 3 - left; 4 - bottom
        	if ( (velocity >= 35 && position == 3 && Color.Equals(mainForm.yellowLeft.BackColor, Color.Yellow))  ||
        	     (velocity >= 35 && position == 2 && Color.Equals(mainForm.yellowRight.BackColor, Color.Yellow)) ||
        	     (velocity >= 35 && position == 1 && Color.Equals(mainForm.yellowTop.BackColor, Color.Yellow))   ||
        	     (velocity >= 35 && position == 4 && Color.Equals(mainForm.yellowBottom.BackColor, Color.Yellow))||
					this is Ambulance )
        	{
        		if (position == 3) mainForm.barrierLeft.BackColor = Color.FromArgb(192, 255, 192);
        		else if (position == 2) mainForm.barrierRight.BackColor = Color.FromArgb(192, 255, 192);
        		else if (position == 1) mainForm.barrierTop.BackColor = Color.FromArgb(192, 255, 192);
        		else if (position == 4) mainForm.barrierBottom.BackColor = Color.FromArgb(192, 255, 192);
        		return;
        	}
        	if (position == 3 && ((currentXY >= 88 && currentXY <= 102) || (velocity >= 35 && currentXY >= 50 && currentXY <= 102)) && mainForm.leftState == false)
        	{
        		T.Stop();
        		T.Enabled = false;
            	LightWait = new System.Timers.Timer(timerInterval);
            	LightWait.Elapsed += TLSecondWait;
            	LightWait.Enabled = true;
            	//MessageBox.Show(position.ToString() + " " + currentXY.ToString() + " " + mainForm.topState.ToString() + " " + mainForm.rightState.ToString() + " " + mainForm.leftState.ToString() + " " + mainForm.bottomState.ToString());
        	}
        	else if (position == 2 && ((currentXY >= 533 && currentXY <= 547) || (velocity >= 35 && currentXY >= 533 && currentXY <= 583 )) && mainForm.rightState == false)
        	{
        		T.Stop();
        		T.Enabled = false;
            	LightWait = new System.Timers.Timer(timerInterval);
            	LightWait.Elapsed += TLSecondWait;
            	LightWait.Enabled = true;
            	//MessageBox.Show(position.ToString() + " " + currentXY.ToString() + " " + mainForm.topState.ToString() + " " + mainForm.rightState.ToString() + " " + mainForm.leftState.ToString() + " " + mainForm.bottomState.ToString());
        	}
        	else if (position == 1 && ((currentXY >= -30 && currentXY <= -16) || (velocity >= 35 && currentXY >= -60 && currentXY <= -16)) && mainForm.topState == false)
        	{
        		T.Stop();
        		T.Enabled = false;
            	LightWait = new System.Timers.Timer(timerInterval);
            	LightWait.Elapsed += TLSecondWait;
            	LightWait.Enabled = true;
            	//MessageBox.Show(position.ToString() + " " + currentXY.ToString() + " " + mainForm.topState.ToString() + " " + mainForm.rightState.ToString() + " " + mainForm.leftState.ToString() + " " + mainForm.bottomState.ToString());
        	}
        	else if (position == 4 && ((currentXY >= 426 && currentXY <= 450) || (velocity >= 35 && currentXY >= 426 && currentXY <= 470)) && mainForm.bottomState == false)
        	{
        		T.Stop();
        		T.Enabled = false;
            	LightWait = new System.Timers.Timer(timerInterval);
            	LightWait.Elapsed += TLSecondWait;
            	LightWait.Enabled = true;
            	//MessageBox.Show(position.ToString() + " " + currentXY.ToString() + " " + mainForm.topState.ToString() + " " + mainForm.rightState.ToString() + " " + mainForm.leftState.ToString() + " " + mainForm.bottomState.ToString());
        	}
        }
        
        public void TLSecondWait(Object sender, ElapsedEventArgs e)
        {
        	waitedOnTrafficLight = true;
        	if (position == 3 && mainForm.leftState == true)
        	{
        		T.Enabled = true;
        		T.Start();
        		LightWait.Stop();
        		LightWait.Enabled = false;
	            if (position == 1) mainForm.topCount--;
	            else if (position == 2) mainForm.rightCount--;
	            else if (position == 3) mainForm.leftCount--;
	            else if (position == 4) mainForm.bottomCount--;
        	}
        	else if (position == 2 && mainForm.rightState == true)
        	{
        		T.Enabled = true;
        		T.Start();
        		LightWait.Stop();
        		LightWait.Enabled = false;
	            if (position == 1) mainForm.topCount--;
	            else if (position == 2) mainForm.rightCount--;
	            else if (position == 3) mainForm.leftCount--;
	            else if (position == 4) mainForm.bottomCount--;
        	}
        	else if (position == 1 && mainForm.topState == true)
        	{
        		T.Enabled = true;
        		T.Start();
        		LightWait.Stop();
        		LightWait.Enabled = false;
	            if (position == 1) mainForm.topCount--;
	            else if (position == 2) mainForm.rightCount--;
	            else if (position == 3) mainForm.leftCount--;
	            else if (position == 4) mainForm.bottomCount--;
        	}
        	else if (position == 4 && mainForm.bottomState == true)
        	{
        		T.Enabled = true;
        		T.Start();
        		LightWait.Stop();
        		LightWait.Enabled = false;
	            if (position == 1) mainForm.topCount--;
	            else if (position == 2) mainForm.rightCount--;
	            else if (position == 3) mainForm.leftCount--;
	            else if (position == 4) mainForm.bottomCount--;
        	}
        	//MessageBox.Show(position.ToString());
        }
    }

    class Car : Vehicle
    {
        public Car(int v, Form1 f, List<Point> t)
        {
            img = new PictureBox();
            img.Size = new Size(160, 160);
            img.Location = new Point(t[0].X, t[0].Y);
            img.BackColor = Color.Transparent;
            
            velocity = 0.05 * v;
            mainForm = f;
            trajectory = t;
            switch (t[0].Y)
            {
            	case -180:
            		position = 1;
            		break;
            	case 130:
            		position = 2;
            		break;
            	case 260:
            		position = 3;
            		break;
            	case 560:
            		position = 4;
            		break;
            	default:
            		break;
            }
            int x1 = t[1].X - t[0].X, y1 = t[1].Y - t[0].Y, x2 = 1, y2 = 0;
            double angle = Math.Atan2(x2 * y1 - y2 * x1, x2 * x1 + y2 * y1);
            if (position == 3 || position == 2) CheckTrafficLight(Convert.ToInt32(x1));
            else CheckTrafficLight(Convert.ToInt32(y1));
                
            img.Image = ImageTools.RotateImage((Bitmap)Properties.Resources.CarImage, (float)(180.0 * angle / Math.PI));
            f.pictureBox1.Controls.Add(img);
            f.pictureBox1.Controls.SetChildIndex(img, 5);

        }
    }

    class Ambulance : Vehicle
    {
        public Ambulance(int v, Form1 f, List<Point> t)
        {
            img = new PictureBox();
            img.Size = new Size(160, 160);
            img.Location = new Point(t[0].X, t[0].Y);
            img.BackColor = Color.Transparent;
            
            velocity = 0.05 * v;
            mainForm = f;
            trajectory = t;
            switch (t[0].Y)
            {
            	case -180:
            		position = 1;
            		break;
            	case 130:
            		position = 2;
            		break;
            	case 260:
            		position = 3;
            		break;
            	case 560:
            		position = 4;
            		break;
            	default:
            		break;
            }
            int x1 = t[1].X - t[0].X, y1 = t[1].Y - t[0].Y, x2 = 1, y2 = 0;
            double angle = Math.Atan2(x2 * y1 - y2 * x1, x2 * x1 + y2 * y1);
            img.Image = ImageTools.RotateImage((Bitmap)Properties.Resources.AmbulanceImage, (float)(180.0 * angle / Math.PI));
            f.pictureBox1.Controls.Add(img);
            f.pictureBox1.Controls.SetChildIndex(img, 0);
        }
    }

    class ImageTools
    {
        public static Image RotateImage(Image img, float rotationAngle)
        {
            //create an empty Bitmap image
            //Bitmap bmp = new Bitmap(img.Width, img.Height);
            Bitmap bmp = new Bitmap(160, 160);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }

    }
}
