using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace P3DHIL
{
    public class WindowInfo
    {
        public int hWnd { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the WindowInfo class.
        /// </summary>
        public WindowInfo(int hWnd, string name)
        {
            this.hWnd = hWnd;
            this.Name = name;
        }
    }
}
