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
using Caliburn.Micro;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using P3DHIL.ExtensionMethods;

namespace P3DHIL
{
    public class SimConnectWrapper : PropertyChangedBase, IDisposable
    {
        private static UdpClient _udpSend = new UdpClient();
        private static Socket _udpRecv;

        private static SimConnect sc = null;

        private EventWaitHandle scReady = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread bgThread = null;
        private uint SIMCONNECT_OBJECT_ID_USER = 0;
        public delegate void MessageProcessDelegate();

        public PlanePosition SimPosition { get; set; }
        public SurfacesPosition SimControl { get; set; }

        public PlaneControl ApmControl { get; set; }

        public bool AutoPilotEnabled { get; set; }
        public bool UseMagneticHeading { get; set; }

        public void ConnectToSimConnect()
        {
            try
            {
                sc = new SimConnect("VE_SC_WPF", (IntPtr)0, 0, scReady, 0);

                sc.OnRecvClientData += sc_OnRecvClientData;
                sc.OnRecvCustomAction += sc_OnRecvCustomAction;
                sc.OnRecvEvent += sc_OnRecvEvent;
                sc.OnRecvEventFilename += sc_OnRecvEventFilename;
                sc.OnRecvEventFrame += sc_OnRecvEventFrame;
                sc.OnRecvEventObjectAddremove += sc_OnRecvEventObjectAddremove;
                sc.OnRecvException += sc_OnRecvException;
                sc.OnRecvGroundInfo += sc_OnRecvGroundInfo;
                sc.OnRecvNull += sc_OnRecvNull;
                sc.OnRecvObserverData += sc_OnRecvObserverData;
                sc.OnRecvOpen += sc_OnRecvOpen;
                sc.OnRecvQuit += sc_OnRecvQuit;
                sc.OnRecvSimobjectData += sc_OnRecvSimobjectData;
                sc.OnRecvSimobjectDataBytype += sc_OnRecvSimobjectDataBytype;
                sc.OnRecvSynchronousBlock += sc_OnRecvSynchronousBlock;
                sc.OnRecvSystemState += sc_OnRecvSystemState;

                _udpRecv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _udpRecv.Bind(new IPEndPoint(IPAddress.Any, 49000));
                
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += OnReceive;
                e.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                e.SetBuffer(new byte[255], 0, 255);

                if (!_udpRecv.ReceiveFromAsync(e))
                    OnReceive(_udpRecv, e);
            }
            catch (COMException)
            {
                Console.WriteLine("Unable to connect to SimConnect");
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            bgThread = new Thread(new ThreadStart(scMessageThread));
            bgThread.IsBackground = true;
            bgThread.Start();
        }

        void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0)
            {
                byte[] buffer = e.Buffer.Take(e.BytesTransferred).ToArray();
                PlaneControl.Struct control = buffer.ByteArrayToStructure<PlaneControl.Struct>(0);

                ApmControl = new PlaneControl(control);

                if (AutoPilotEnabled)
                {
                    SurfacesPosition.Struct setPosition = new SurfacesPosition.Struct();
                    setPosition.throttle = ApmControl.Throttle;
                    setPosition.rudder = ApmControl.Rudder;
                    setPosition.elevator = ApmControl.Pitch;
                    setPosition.aileron = ApmControl.Roll;

                    sc.SetDataOnSimObject(Definitions.SurfacesPosition, SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, setPosition);
                }

                NotifyOfPropertyChange(() => ApmControl);
            }

            if (!_udpRecv.ReceiveFromAsync(e))
                OnReceive(_udpRecv, e);
        }

        void sc_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            Console.WriteLine("sc_OnRecvSystemState");
            data.Dump();
        }

        void sc_OnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            Console.WriteLine("sc_OnRecvSystemState");
        }

        void sc_OnRecvSynchronousBlock(SimConnect sender, SIMCONNECT_RECV_SYNCHRONOUS_BLOCK data)
        {
            Console.WriteLine("sc_OnRecvSynchronousBlock");
        }

        void sc_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint)Requests.PlanePosition)
            {
                PlanePosition.Struct pos = (PlanePosition.Struct)data.dwData[0];
                
                if (UseMagneticHeading)
                    pos.heading = pos.magheading;

                SimPosition = new PlanePosition(pos);

                byte[] packet = P3DHIL.ExtensionMethods.StructExtensionMethods.StructureToByteArray(pos);
                
                _udpSend.SendAsync(packet, packet.Length, "127.0.0.1", 49005).ContinueWith((x) => { Console.WriteLine("UDP Send [{0}|{1}] {2}", x.Result, packet.Length, x.Exception.Message); }, TaskContinuationOptions.NotOnRanToCompletion);

                NotifyOfPropertyChange(() => SimPosition);
            }
            else if (data.dwRequestID == (uint)Requests.SurfacesPosition)
            {
                SurfacesPosition.Struct pos = (SurfacesPosition.Struct)data.dwData[0];
                
                SimControl = new SurfacesPosition(pos);
                NotifyOfPropertyChange(() => SimControl);
            }
            else
            {
                Console.WriteLine("Unknown");
            }
        }

        void sc_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("sc_OnRecvQuit");
        }

        void sc_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("sc_OnRecvOpen");
            data.PrintDump();
        }

        void sc_OnRecvObserverData(SimConnect sender, SIMCONNECT_RECV_OBSERVER_DATA data)
        {
            Console.WriteLine("sc_OnRecvObserverData");
            data.PrintDump();
        }

        void sc_OnRecvNull(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("sc_OnRecvNull");
        }

        void sc_OnRecvGroundInfo(SimConnect sender, SIMCONNECT_RECV_GROUND_INFO data)
        {
            Console.WriteLine("sc_OnRecvGroundInfo");
        }

        void sc_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.WriteLine("sc_OnRecvException");
        }

        void sc_OnRecvEventObjectAddremove(SimConnect sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data)
        {
            Console.WriteLine("sc_OnRecvEventObjectAddremove");
        }

        void sc_OnRecvEventFrame(SimConnect sender, SIMCONNECT_RECV_EVENT_FRAME data)
        {
            Console.WriteLine("sc_OnRecvEventFrame");
        }

        void sc_OnRecvEventFilename(SimConnect sender, SIMCONNECT_RECV_EVENT_FILENAME data)
        {
            Console.WriteLine("sc_OnRecvEventFilename");
        }

        void sc_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            Console.WriteLine("sc_OnRecvEvent");
        }

        void sc_OnRecvCustomAction(SimConnect sender, SIMCONNECT_RECV_CUSTOM_ACTION data)
        {
            Console.WriteLine("sc_OnRecvCustomAction");
        }

        void sc_OnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
            Console.WriteLine("sc_OnRecvClientData");
        }

        private void scMessageThread()
        {
            while (true)
            {
                scReady.WaitOne();

                Dispatcher.CurrentDispatcher.Invoke(new MessageProcessDelegate(scMessageProcess));
            }
        }
 
        private void scMessageProcess()
        {
            sc.ReceiveMessage();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (scReady != null)
                {
                    scReady.Dispose();
                    scReady = null;
                }
        }
        
        ~SimConnectWrapper()
        {
            Dispose(false);
        }
 

        public void Setup()
        {
            sc.AddToDataDefinition(Definitions.PlanePosition, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "Plane Altitude", "meters", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

            sc.AddToDataDefinition(Definitions.PlanePosition, "Plane Bank Degrees", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "Plane Pitch Degrees", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "PLANE HEADING DEGREES TRUE", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            
            sc.AddToDataDefinition(Definitions.PlanePosition, "INCIDENCE ALPHA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "INCIDENCE BETA", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

            //sitldata.rollRate = p3d.phidot * rad2deg;
            //sitldata.pitchRate = p3d.thetadot * rad2deg;
            //sitldata.yawRate = p3d.psidot * rad2deg;

            //sitldata.speedN = p3d.v_north * ft2m;
            //sitldata.speedE = p3d.v_east * ft2m;
            //sitldata.speedD = p3d.v_down * ft2m;

            //sitldata.xAccel = (p3d.A_X_pilot * 9.808 / 32.2); // pitch
            //sitldata.yAccel = (p3d.A_Y_pilot * 9.808 / 32.2); // roll
            //sitldata.zAccel = (p3d.A_Z_pilot / 32.2 * 9.808);

            sc.AddToDataDefinition(Definitions.PlanePosition, "AIRSPEED TRUE", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.PlanePosition, "VERTICAL SPEED", "feet per second", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.RegisterDataDefineStruct<PlanePosition.Struct>(Definitions.PlanePosition);
            sc.RequestDataOnSimObject(Requests.PlanePosition, Definitions.PlanePosition, SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);

            sc.AddToDataDefinition(Definitions.SurfacesPosition, "GENERAL ENG THROTTLE LEVER POSITION:1", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.SurfacesPosition, "RUDDER POSITION", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.SurfacesPosition, "ELEVATOR POSITION", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
            sc.AddToDataDefinition(Definitions.SurfacesPosition, "AILERON POSITION", "percent", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);

            sc.RegisterDataDefineStruct<SurfacesPosition.Struct>(Definitions.SurfacesPosition);
            sc.RequestDataOnSimObject(Requests.SurfacesPosition, Definitions.SurfacesPosition, SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.SIM_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 0, 0);
        }
    }
}
