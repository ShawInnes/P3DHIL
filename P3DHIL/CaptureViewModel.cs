using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Spazzarama.ScreenCapture;
using System.Windows.Threading;
using System.Windows;

namespace P3DHIL
{
    public class CaptureViewModel : PropertyChangedBase
    {
        public delegate bool WindowEnumCallback(int hwnd, int lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(WindowEnumCallback lpEnumFunc, int lParam);

        [DllImport("user32.dll")]
        public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(int h);

        public ObservableCollection<WindowInfo> Windows { get; set; }

        private bool AddWnd(int hwnd, int lparam)
        {
            if (IsWindowVisible(hwnd))
            {
                StringBuilder sb = new StringBuilder(255);
                GetWindowText(hwnd, sb, sb.Capacity);
                Windows.Add(new WindowInfo(hwnd, sb.ToString()));
            }
            return true;
        }

        public BitmapSource CapturedImage { get; set; }
        public string DebugText { get; set; }

        public WindowInfo FindWindows(string name)
        {
            EnumWindows(new WindowEnumCallback(this.AddWnd), 0);
            
            return Windows.FirstOrDefault(p => p.Name.StartsWith(name));
        }

        /// <summary>
        /// Initializes a new instance of the CaptureViewModel class.
        /// </summary>
        public CaptureViewModel()
        {
            Windows = new ObservableCollection<WindowInfo>();

            System.Timers.Timer tim = new System.Timers.Timer(500);
            tim.Elapsed += (a, b) =>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (App.SimAccess.SimPosition == null)
                    {
                        DebugText = "SimConnect not operating";
                        NotifyOfPropertyChange(() => DebugText);
                        return;
                    }

                    WindowInfo findWindows = FindWindows("NADIR Camera");
                    if (findWindows != null)
                    {
                        using (Bitmap bitmap = Direct3DCapture.CaptureWindow(new IntPtr(findWindows.hWnd)))
                        {
                            CapturedImage = bitmap.ToWpfBitmap();
                            
                            PlanePosition pos = App.SimAccess.SimPosition;
                            
                            string fileName = string.Format("{0:000.000}{1:000.000}.jpg", pos.Longitude, pos.Latitude);;
                            DebugText = fileName;
                            string directory = @"C:\Jobs\NADIR";

                            if (!System.IO.Directory.Exists(directory))
                                System.IO.Directory.CreateDirectory(directory);

                            bitmap.Save(System.IO.Path.Combine(directory, fileName), System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        NotifyOfPropertyChange(() => CapturedImage);
                        NotifyOfPropertyChange(() => DebugText);
                    }
                    else
                    {
                        DebugText = "unable to locate window";
                        NotifyOfPropertyChange(() => DebugText);
                    }
                }), DispatcherPriority.Background);
            };
            tim.Start();
        }
    }
}
