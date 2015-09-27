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
    public enum Requests
    {
        PlanePosition,
        SurfacesPosition
    }
}
