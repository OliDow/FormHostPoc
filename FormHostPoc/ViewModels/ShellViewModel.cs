using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using FormHostPoc.Helpers;

namespace FormHostPoc.ViewModels
{
    public class ShellViewModel : BaseNotify
    {
        public ShellViewModel()
        {
            WindowWidth = 600;
            WindowHeight = 500;
            MainTitle = "Main Title";
            BreadCrumb = "NA";
            IsLoading = true;

            Task.Factory.StartNew(() =>
            {
                //Process p = OpenProcess(@"C:\\Windows\\System32\\notepad.exe");
                Process p = OpenProcess(@"C:\\Program Files\\WinRAR\\WinRar.exe");

                return p;

            }).ContinueWith(r =>
            {
                try
                {
                    Process p = r.Result;

                    _handler = p.MainWindowHandle;
                    _host.Child = _panel;
                    Content = _host;

                    int dwStyle = GetWindowLong(_handler, GWL_STYLE);
                    SetParent(_handler, _panel.Handle);
                    SetWindowLong(_handler, GWL_STYLE, new IntPtr(dwStyle & ~WS_CAPTION & ~WS_THICKFRAME));
                    SetWindowPos(_handler, IntPtr.Zero, 0, 0, (int) Math.Round(WindowWidth), (int) Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);

                    BreadCrumb += " Host: " + p.Id.ToString(CultureInfo.InvariantCulture);

                    IsLoading = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

            }, UiTaskSchedulerHelper.Instance.UiTaskScheduler);
        }

        private Process OpenProcess(string path)
        {

            Process p = null;
            lock (locked)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = path;
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Minimized;
                p = Process.Start(psi);
                BreadCrumb += " OpenProcess: " + p.Id;

                p.EnableRaisingEvents = true;
                p.WaitForInputIdle();

            }
            return p;
        }


        private double _windowWidth;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                Set(() => WindowWidth, ref _windowWidth, value);
                SetWindowPos(_handler, IntPtr.Zero, 0, 0, (int)Math.Round(WindowWidth), (int)Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);
            }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                Set(() => WindowHeight, ref _windowHeight, value);
                SetWindowPos(_handler, IntPtr.Zero, 0, 0, (int)Math.Round(WindowWidth), (int)Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(() => IsLoading, ref _isLoading, value); }
        }

        private string _mainTitle;
        public string MainTitle
        {
            get { return _mainTitle; }
            set { Set(() => MainTitle, ref _mainTitle, value); }
        }

        private string _breadCrumb;
        public string BreadCrumb
        {
            get { return _breadCrumb; }
            set { Set(() => BreadCrumb, ref _breadCrumb, value); }
        }

        private Process _process;
        public Process ProccessBind
        {
            get { return _process; }
            set { Set(() => ProccessBind, ref _process, value); }
        }

        private object _content;
        public object Content
        {
            get { return _content; }
            set { Set(() => Content, ref _content, value); }
        }

        private Object locked = new Object();


        private readonly System.Windows.Forms.Panel _panel = new System.Windows.Forms.Panel();
        private readonly WindowsFormsHost _host = new WindowsFormsHost();
        private IntPtr _handler;

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);


        // ReSharper disable InconsistentNaming
        //Recalculate the size and position of the windows client area.
        const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        //set a new Window Style (Border, has horizontal/vertical scrollbar, please see MSDN)
        static readonly int GWL_STYLE = (-16);
        //the specified window has a title bar
        public const uint WS_CAPTION = 0x00C00000;
        //Creates a window with a resizable frame.
        public const uint WS_THICKFRAME = 0x00040000;
        //If the parent window is redrawn,the area which is covered by the "child" windows will be excluded.
        public const uint WS_CLIPCHILDREN = 0x02000000;
        //Create a new overlapped window,take care that it has a titlebar and border per default.
        public const uint WS_OVERLAPPED = 0x00000000;
        // ReSharper restore InconsistentNaming

    }
}
