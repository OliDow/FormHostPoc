using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using FormHostPoc.Helpers;
using Microsoft.Practices.Prism.Commands;
using System.Globalization;
using Application = System.Windows.Application;
using System.Timers;

namespace FormHostPoc.ViewModels
{
    public class ShellViewModel : BaseNotify
    {
        public ShellViewModel()
        {
            TestCommand = new DelegateCommand(OnTestCommand);
            CloseCommand = new DelegateCommand(OnCloseCommand);
            WindowWidth = 900;
            WindowHeight = 700;
            MainTitle = "Main Title";
            BreadCrumb = "Main" + Process.GetCurrentProcess().Id;
            //IsLoading = true;

            _pr = new Process();
            ProcessStartInfo prs = new ProcessStartInfo();
            //prs.FileName = @"I:\Exec\a\a10.exe";
            //prs.FileName = @"I:\Exec\j\j01.exe";
            prs.FileName = @"c:\\SlowStarter.exe";
            //prs.FileName = @"C:\\Program Files (x86)\\Fiddler2\\Fiddler.exe";
            //prs.FileName = @"C:\\Program Files\\WinRAR\\WinRar.exe";
            //prs.FileName = @"C:\\Windows\\System32\\notepad.exe";
            //prs.UseShellExecute = true;
            prs.WindowStyle = ProcessWindowStyle.Minimized;
            _pr.StartInfo = prs;

  _host.Child = _panel;
                    Content = _host;


            ThreadStart ths = new ThreadStart(() =>
            { 
                _pr.Start();
                _pr.WaitForInputIdle();

                //Thread.Sleep(2000);


                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Console.WriteLine("Info");
                    IntPtr win = GetActiveWindow();
                    var ctrl = GetFocus();
                    var wnd = FindWindowsWithText(_pr.Id, "Contract Maintenance");
                    _handler = _pr.MainWindowHandle;
                    IntPtr _childHandle = wnd;

                    Console.WriteLine(@"Win: {0} - Ctrl - {1}", win, ctrl);
                    BreadCrumb += string.Format(" | Host: {0} / {1} | handler: {2}", +_pr.Id, _pr.MainWindowHandle, _handler);


                    int dwStyle = GetWindowLong(_pr.MainWindowHandle, GWL_STYLE);
                    SetParent(_childHandle, _panel.Handle);
                    //SetWindowLong(_childHandle, GWL_STYLE, new IntPtr(dwStyle & ~WS_CAPTION & ~WS_THICKFRAME));
                    ShowWindow(_childHandle, SW_SHOWMAXIMIZED);
                    SetWindowPos(_childHandle, IntPtr.Zero, 0, -22, (int)Math.Round(WindowWidth), (int)Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);

                    Console.WriteLine(@"Win: {0} - Ctrl - {1}", win, ctrl);
                    IsLoading = false;

                    aTimer = new System.Timers.Timer(5);
                    aTimer.Interval = 2000;
                    aTimer.Enabled = true;

                    aTimer.Elapsed += (sender, args) =>
                    {
                        IntPtr ancestor = GetAncestor(_handler, GA_ROOT);
                        Console.WriteLine(ancestor == IntPtr.Zero ? "Closed" : "Open");
                    };

                }));

            });
            Thread th = new Thread(ths);
            th.Start();





            //            Task.Factory.StartNew(() =>
            //            {
            //                //Process p = OpenProcess(@"C:\\Windows\\System32\\notepad.exe");
            //                Process p = OpenProcess(@"C:\\Program Files\\WinRAR\\WinRar.exe");
            //
            //                return p;
            //
            //            }).ContinueWith(r =>
            //            {
            //                try
            //                {
            //                    Process p = r.Result;
            //
            //                    _handler = p.MainWindowHandle;
            //                    _host.Child = _panel;
            //                    Content = _host;
            //
            //                    int dwStyle = GetWindowLong(_handler, GWL_STYLE);
            //                    SetParent(_handler, _panel.Handle);
            //                    SetWindowLong(_handler, GWL_STYLE, new IntPtr(dwStyle & ~WS_CAPTION & ~WS_THICKFRAME));
            //                    SetWindowPos(_handler, IntPtr.Zero, 0, 0, (int)Math.Round(WindowWidth), (int)Math.Round(WindowHeight) - 106, SWP_FRAMECHANGED);
            //
            //                    BreadCrumb += " Host: " + p.Id.ToString(CultureInfo.InvariantCulture);
            //
            //                    IsLoading = false;
            //                }
            //                catch (Exception e)
            //                {
            //                    MessageBox.Show(e.Message);
            //                }
            //
            //            }, UiTaskSchedulerHelper.Instance.UiTaskScheduler);
        }

        private void OnCloseCommand()
        {
            _pr.Kill();
//            IntPtr pIntPrt = GetLastActivePopup(Process.GetCurrentProcess().MainWindowHandle);
//            IntPtr cIntPrt = GetLastActivePopup(_handler);
//
//            Console.WriteLine(@"IntPrt: {0} | LastActive: {1}/{2} ", _handler, pIntPrt, cIntPrt);
//
//            MessageBox.Show((_handler == cIntPrt).ToString());

        }

        private void OnTestCommand()
        {
            IntPtr pIntPrt = GetLastActivePopup(Process.GetCurrentProcess().MainWindowHandle);
            IntPtr cIntPrt = GetLastActivePopup(_handler);

            IntPtr win = GetActiveWindow();
            var ctrl = GetFocus();
            bool popup = AnyPopup();
            bool toTop = BringWindowToTop(pIntPrt);
            IntPtr ancestor = GetAncestor(_handler, GA_ROOT);

            //IntPtr parent = GetParent(_handler);

            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
          

            EnumWindowsProc childProc = new EnumWindowsProc(EnumWindow);
            EnumChildWindows(_handler, childProc, GCHandle.ToIntPtr(listHandle));
            IntPtr pGetTop = GetTopWindow(Process.GetCurrentProcess().MainWindowHandle);
            IntPtr cGetTop = GetTopWindow(_handler);

            Console.WriteLine(@"IntPrt: {0} | Win: {1} | Ctrl: {2} | Popup: {3} | BringToTop: {4} | Process: {5}/{6}/{7} | Ancestor : {8} |  LastActive: {9}/{10} | GetTopWindow: {11}/{12}", _handler, win, ctrl, popup, toTop, _pr.Id, _pr.MainWindowHandle, _pr.HandleCount, ancestor, pIntPrt, cIntPrt, cGetTop, pGetTop);

        }

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

        public  IntPtr FindWindowsWithText(int processId, string titleText)
        {
            var enumerateProcessWindowHandles = EnumerateProcessWindowHandles(processId);
            if (enumerateProcessWindowHandles != null)
            {
                List<IntPtr> windowList = enumerateProcessWindowHandles.ToList();
                foreach (var wnd in windowList)
                {

                    uint processID = 0;
                    uint PID = GetWindowThreadProcessId((IntPtr)wnd, out processID);

                    var currentProcessId = new IntPtr(processID);
                    var verifyProcessId = (int)currentProcessId;
                    var windowText = GetWindowText(wnd);
                    if (processID == processId && windowText.Contains(titleText))
                        return wnd;

                    //if (windowText.Contains(titleText))
                    //{
                    //    return wnd;
                    //}
                }
            }

            return IntPtr.Zero;
        }

        public  IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
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
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        private Process OpenProcess(string path)
        {

            Process p = new Process();
            //            lock (locked)
            //            {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = path;
            psi.UseShellExecute = true;
            psi.WindowStyle = ProcessWindowStyle.Minimized;

            p.StartInfo = psi;


            ThreadStart ths = new ThreadStart(() =>
            {
                p.Start();
            });
            Thread th = new Thread(ths);
            th.Start();

            //p = Process.Start(psi);
            BreadCrumb += " OpenProcess: " + p.Id;

            p.EnableRaisingEvents = true;
            p.WaitForInputIdle();

            //}
            return p;
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

        private Object locked = new Object();

        #endregion

        private Process _pr;
        private static System.Timers.Timer aTimer;

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
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        static extern bool AnyPopup();
        [DllImport("user32.dll")]
        static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWndCild);
        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWndCild);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hWnd, int flags);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        //
        //        private static ArrayList GetAllWindows()
        //        {
        //            var windowHandles = new ArrayList();
        //            EnumedWindow callBackPtr = GetWindowHandle;
        //            EnumWindows(callBackPtr, windowHandles);
        //
        //            foreach (IntPtr windowHandle in windowHandles.ToArray())
        //            {
        //                EnumChildWindows(windowHandle, callBackPtr, windowHandles);
        //            }
        //
        //            return windowHandles;
        //   }

        //        private delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);
        //
        //        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //        [return: MarshalAs(UnmanagedType.Bool)]
        //        private static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList lParam);
        //
        //        [DllImport("user32")]
        //        [return: MarshalAs(UnmanagedType.Bool)]
        //        private static extern bool EnumChildWindows(IntPtr window, EnumedWindow callback, ArrayList lParam);
        //
        //        private static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
        //        {
        //            windowHandles.Add(windowHandle);
        //            return true;
        //        }



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
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;

        private const int GA_PARENT = 1;
        private const int GA_ROOT = 2;
        private const int GA_ROOTOWNER = 3;
        // ReSharper restore InconsistentNaming

        #region COmmand
        public DelegateCommand TestCommand { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }
        #endregion
    }
}
