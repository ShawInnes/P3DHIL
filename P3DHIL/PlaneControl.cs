using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LockheedMartin.Prepar3D.SimConnect;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using ServiceStack.Text;

namespace P3DHIL
{
    public class PlaneControl
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Struct
        {
            public double roll;
            public double pitch;
            public double rudder;
            public double throttle;
        }

        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Rudder { get; set; }
        public double Throttle { get; set; }

        /// <summary>
        /// Initializes a new instance of the PlaneControl class.
        /// </summary>
        public PlaneControl()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the PlaneControl class.
        /// </summary>
        public PlaneControl(Struct pkt)
        {
            double ScaleFactor = 1.0;
            
            Roll = (pkt.roll / ScaleFactor) * 100.0;
            Pitch = (pkt.pitch / ScaleFactor) * 100.0;
            Rudder = (pkt.rudder / ScaleFactor) * 100.0;

            // Throttle must be reversed
            Throttle = (pkt.throttle / ScaleFactor) * 100.0;
        }
    }
}
