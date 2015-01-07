using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using FormHostPoc.Helpers;
using Microsoft.Practices.Prism.Commands;

namespace FormHostPoc.ViewModels
{
    public class ShellViewModel : BaseNotify
    {
        public ShellViewModel()
        {
            //TestCommand = new DelegateCommand(OnTestCommand);
            VB6ClassName = "ThunderRT6FormDC";
            CloseCommand = new DelegateCommand(OnCloseCommand);
            MainTitle = "Main Title";
            BreadCrumb = "Main" + Process.GetCurrentProcess().Id;
            _host.Child = _panel;
            Content = _host;

            Task.Factory.StartNew(() =>
            {
                _pr = new Process { StartInfo = { FileName = @"I:\Exec\t\t11.exe" } };
                _pr.Start();
                _pr.WaitForInputIdle();

            }).ContinueWith(r =>
            {

                Console.WriteLine("Info");
                var wnd = FindWindowsWithClassName(_pr.Id, VB6ClassName);
                _handler = (IntPtr)_pr.Id;
                IntPtr childHandle = wnd;

                BreadCrumb += string.Format(" | Host: {0} / {1} | handler: {2}", +_pr.Id, _pr.MainWindowHandle,
                _handler);
                SetParent(childHandle, _panel.Handle);

                RECT rct;
                if (GetWindowRect(new HandleRef(this, childHandle), out rct))
                {
                    WindowWidth = rct.right - rct.left;
                    WindowHeight = rct.bottom - rct.top + 106;
                }
                SetWindowPos(childHandle, IntPtr.Zero, 0, -22, (int)Math.Round(WindowWidth),
                (int)Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);

                IsLoading = false;
                aTimer = new System.Timers.Timer(5);
                aTimer.Interval = 2000;
                aTimer.Enabled = true;
                aTimer.Elapsed += (sender, args) =>
                {
                    IntPtr ancestor = GetAncestor(childHandle, GA_ROOT);
                    Console.WriteLine(ancestor == IntPtr.Zero ? "Closed" : "Open");
                };

            }, UiTaskSchedulerHelper.Instance.UiTaskScheduler);

        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]

        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;

            /// <summary> Win32 </summary>
            public int top;

            /// <summary> Win32 </summary>
            public int right;

            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); } // Abs needed for BIDI OS
            }

            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }

            
        }

        private void OnCloseCommand()
        {

            var wnd = FindWindowsWithClassName(_pr.Id, VB6ClassName);
            if (wnd != IntPtr.Zero)
            {
                MessageBox.Show("There are child windows opened, do you wish to close all?", "Warning",
                    MessageBoxButton.OKCancel);
            }
            else
            {

                _pr.Kill();
            }
        }

        //private void OnTestCommand()
        //{
        //    IntPtr pIntPrt = GetLastActivePopup(Process.GetCurrentProcess().MainWindowHandle);
        //    IntPtr cIntPrt = GetLastActivePopup(_handler);

        //    IntPtr win = GetActiveWindow();
        //    var ctrl = GetFocus();
        //    bool popup = AnyPopup();
        //    bool toTop = BringWindowToTop(pIntPrt);
        //    IntPtr ancestor = GetAncestor(_handler, GA_ROOT);

        //    //IntPtr parent = GetParent(_handler);

        //    List<IntPtr> result = new List<IntPtr>();
        //    GCHandle listHandle = GCHandle.Alloc(result);


        //    EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
        //    EnumChildWindows(_handler, childProc, GCHandle.ToIntPtr(listHandle));
        //    IntPtr pGetTop = GetTopWindow(Process.GetCurrentProcess().MainWindowHandle);
        //    IntPtr cGetTop = GetTopWindow(_handler);

        //    Console.WriteLine(@"IntPrt: {0} | Win: {1} | Ctrl: {2} | Popup: {3} | BringToTop: {4} | Process: {5}/{6}/{7} | Ancestor : {8} |  LastActive: {9}/{10} | GetTopWindow: {11}/{12}", _handler, win, ctrl, popup, toTop, _pr.Id, _pr.MainWindowHandle, _pr.HandleCount, ancestor, pIntPrt, cIntPrt, cGetTop, pGetTop);



        //    var wnd = FindWindowsWithClassName(_pr.Id, "ThunderRT6FormDC");
        //    //_handler = (IntPtr)_pr.Id;
        //    //IntPtr _childHandle = wnd;

        //    StringBuilder buffer = new StringBuilder(128);

        //    //GetClassName(_childHandle, buffer, buffer.Capacity);

        //    if (wnd!=IntPtr.Zero)
        //    {
        //        System.Windows.MessageBox.Show("There are child windows opened, do you wish to close all?","Warning",MessageBoxButton.OKCancel);
        //    }

        //}

        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size++ > 0)
            {
                var builder = new StringBuilder(size);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);


        public IntPtr FindWindowsWithClassName(int processId, string className)
        {
            var enumerateProcessWindowHandles = EnumerateProcessWindowHandles(processId);
            if (enumerateProcessWindowHandles != null)
            {
                List<IntPtr> windowList = enumerateProcessWindowHandles.ToList();
                foreach (var wnd in windowList)
                {
                    uint processIdOut;
                    GetWindowThreadProcessId((IntPtr)wnd, out processIdOut);

                    var buffer = new StringBuilder(128);
                    GetClassName(wnd, buffer, buffer.Capacity);
                    if (processIdOut == processId && buffer.ToString() == className)
                        return wnd;

                }
            }

            return IntPtr.Zero;
        }

        public IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            try
            {
                var handles = new List<IntPtr>();


                var processThreadCollection = Process.GetProcessById(processId);
                {
                    var processThreads = processThreadCollection.Threads;
                    foreach (ProcessThread thread in processThreads)
                        EnumThreadWindows(thread.Id,
                            (hWnd, lParam) =>
                            {

                                handles.Add(hWnd);
                                return true;
                            }, IntPtr.Zero);
                }
                return handles;
            }
            catch (ArgumentException ex)
            {
                return null;
            }
        }

        #region WindowProperties

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

        #endregion

        private Process _pr;
        private static System.Timers.Timer aTimer;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);

        private readonly System.Windows.Forms.Panel _panel = new System.Windows.Forms.Panel();
        private readonly WindowsFormsHost _host = new WindowsFormsHost();
        private IntPtr _parentHandler;
        private IntPtr _handler;

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int flags);
        // ReSharper disable InconsistentNaming
        const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */

        private const int GA_ROOT = 2;
        private static string VB6ClassName = "ThunderRT6FormDC";

        #region COmmand
        public DelegateCommand TestCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }
        #endregion


    }


}
