using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P3DHIL
{
    public class SurfacesPosition
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Struct
        {
            public double throttle;
            public double rudder;
            public double elevator;
            public double aileron;
        }

        public double Throttle { get; set; }
        public double Rudder { get; set; }
        public double Elevator { get; set; }
        public double Aileron { get; set; }

        /// <summary>
        /// Initializes a new instance of the ThrottlePosition class.
        /// </summary>
        public SurfacesPosition()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the PlanePosition class.
        /// </summary>
        public SurfacesPosition(Struct pos)
        {
            Throttle = pos.throttle;
            Rudder = pos.rudder;
            Elevator = pos.elevator;
            Aileron = pos.aileron;
        }
    }
}
