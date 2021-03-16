using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System;

namespace Conway
{
    class Camera
    {
        public Vector2 Position;
        public int Zoom;
        Thread ThreadedCamera;

        public Camera ()
        {
            Position = new Vector2(0, 0);
            Zoom = 6;
            ThreadedCamera = new Thread(cameraThread);
            ThreadedCamera.Start();
        }

        void cameraThread ()
        {
            while (true)
            {
                Console.WriteLine("In camera loop!");
            }
        }

/*
        Stopwatch _stopWatch = new Stopwatch();
        _stopWatch.Start();
        while (true)
        {
            CameraPosition.X += 1 * _stopWatch.ElapsedMilliseconds;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) CameraPosition.X -= 1 * _stopWatch.ElapsedMilliseconds;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) CameraPosition.Y += 1 * _stopWatch.ElapsedMilliseconds;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) CameraPosition.Y -= 1 * _stopWatch.ElapsedMilliseconds;
            if (Mouse.GetState().ScrollWheelValue > _oldMouseWheelValue && CellScale < 10) //If the wheel has increased
            {
                CellScale++;
                //Math.Clamp(CellScale, 1, 10); //Make sure it does not enter extremes
                refreshTextures();
            }
            else if (Mouse.GetState().ScrollWheelValue < _oldMouseWheelValue && CellScale > 1) //If the wheel has decreased
            {
                CellScale--;
                //Math.Clamp(CellScale, 1, 10); //Make sure it does not enter extremes
                refreshTextures();
            }
            _oldMouseWheelValue = Mouse.GetState().ScrollWheelValue; //Set new _old value for mouse wheel

            Console.WriteLine(Mouse.GetState().ScrollWheelValue);
            Console.WriteLine(CellScale);
            _stopWatch.Reset();
        }*/
    }
}