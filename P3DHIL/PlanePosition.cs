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
    public class PlanePosition
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Struct
        {
            public double longitude;    // geodetic (radians)
            public double latitude;		// geodetic (radians)
            public double altitude;		// above sea level (meters)

            public double phi;			// roll (radians)
            public double theta;		// pitch (radians)
            public double heading;		// yaw or true heading (radians)
            public double magheading;	// magnetic heading (radians)

            public double alpha;         // angle of attack (radians)
            public double beta;          // side slip angle (radians)

            //// Velocities
            //public double phidot;		// roll rate (radians/sec)
            //public double thetadot;		// pitch rate (radians/sec)
            //public double psidot;		// yaw rate (radians/sec)
            public double vcas;		    // calibrated airspeed
            public double climb_rate;	// feet per second
            //public double v_north;       // north velocity in local/body frame, fps
            //public double v_east;        // east velocity in local/body frame, fps
            //public double v_down;        // down/vertical velocity in local/body frame, fps

            //// Accelerations
            //public double A_X_pilot;		// X accel in body frame ft/sec^2
            //public double A_Y_pilot;		// Y accel in body frame ft/sec^2
            //public double A_Z_pilot;		// Z accel in body frame ft/sec^2
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double MagneticHeading { get; set; }

        public double Airspeed { get; set; }
        public double VerticalSpeed { get; set; }

        /// <summary>
        /// Initializes a new instance of the PlanePosition class.
        /// </summary>
        public PlanePosition(Struct pos)
        {
            this.Latitude = pos.latitude;
            this.Longitude = pos.longitude;
            this.Altitude = pos.altitude;

            this.Roll = pos.phi;
            this.Pitch = pos.theta;
            this.Yaw = pos.heading;
            this.MagneticHeading = pos.magheading;

            this.Airspeed = pos.vcas;
            this.VerticalSpeed = pos.climb_rate;
        }
    }
}
