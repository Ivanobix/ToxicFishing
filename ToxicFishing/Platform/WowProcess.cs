using log4net;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace ToxicFishing.Platform
{
    public static class WowProcess
    {
        public static readonly ILog logger = LogManager.GetLogger("Fishbot");

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private static readonly Random random = new();
        private static readonly int LootDelay = 2000;


        public static bool IsWowClassic()
        {
            Process? wowProcess = Get();
            return wowProcess != null && wowProcess.ProcessName.ToLower().Contains("classic"); ;
        }

        //Get the wow-process, if success returns the process else null
        public static Process? Get(string name = "")
        {
            List<string> names = string.IsNullOrEmpty(name) ? new List<string> { "Wow", "WowClassic", "Wow-64", "felsong-64" } : new List<string> { name };

            Process[] processList = Process.GetProcesses();
            foreach (Process? p in processList)
            {
                if (names.Select(s => s.ToLower()).Contains(p.ProcessName.ToLower()))
                    return p;
            }

            logger.Error($"Failed to find the wow process, tried: {string.Join(", ", names)}");

            return null;
        }

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        private static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            _ = GetWindowThreadProcessId(hwnd, out uint pid);
            return Process.GetProcessById((int)pid);
        }

        private static void KeyDown(ConsoleKey key)
        {
            Process? wowProcess = Get();
            if (wowProcess != null)
                _ = PostMessage(wowProcess.MainWindowHandle, WM_KEYDOWN, (int)key, 0);
        }

        public static void PressKey(ConsoleKey key)
        {
            KeyDown(key);
            Thread.Sleep(50 + random.Next(0, 75));
            KeyUp(key);
        }

        public static void KeyUp(ConsoleKey key)
        {
            Process? wowProcess = Get();
            if (wowProcess != null)
                _ = PostMessage(wowProcess.MainWindowHandle, WM_KEYUP, (int)key, 0);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        public static void RightClickMouse(ILog logger, Point position)
        {
            RightClickMouse_LiamCooper(logger, position);
        }

        public static void RightClickMouse_LiamCooper(ILog logger, Point position)
        {
            Process activeProcess = GetActiveProcess();
            Process? wowProcess = Get();
            if (wowProcess != null)
            {
                mouse_event((int)MouseEventFlags.RightUp, position.X, position.Y, 0, 0);
                Point oldPosition = Cursor.Position;

                Thread.Sleep(200);
                Cursor.Position = position;
                Thread.Sleep(LootDelay);
                mouse_event((int)MouseEventFlags.RightDown, position.X, position.Y, 0, 0);
                Thread.Sleep(30 + random.Next(0, 47));
                mouse_event((int)MouseEventFlags.RightUp, position.X, position.Y, 0, 0);
                RefocusOnOldScreen(logger, activeProcess, wowProcess, oldPosition);
                Thread.Sleep(LootDelay / 2);
            }
        }

        private static void RefocusOnOldScreen(ILog logger, Process activeProcess, Process wowProcess, Point oldPosition)
        {
            try
            {
                if (activeProcess.MainWindowTitle != wowProcess.MainWindowTitle)
                {
                    _ = PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
                    Thread.Sleep(30);
                    _ = PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

                    KeyDown(ConsoleKey.Escape);
                    Thread.Sleep(30);
                    KeyUp(ConsoleKey.Escape);

                    Cursor.Position = oldPosition;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }
    }
}