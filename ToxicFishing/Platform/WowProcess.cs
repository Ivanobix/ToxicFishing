using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ToxicFishing.Platform
{
    public static class WowProcess
    {
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private static readonly Random random = new();
        private static readonly int LootDelay = 2000;

        private static Process? cachedWowProcess;

        public static Process? GetWowProcess(string name = "")
        {
            if (cachedWowProcess != null && !cachedWowProcess.HasExited)
                return cachedWowProcess;

            string[] names = string.IsNullOrEmpty(name)
                ? new[] { "wow", "wowclassic", "wow-64", "felsong-64" }
                : new[] { name };

            cachedWowProcess = Process.GetProcesses().FirstOrDefault(x => names.Contains(x.ProcessName.ToLowerInvariant()));
            
            if (cachedWowProcess == null)
                Console.WriteLine($"Failed to find the wow process, tried: {string.Join(", ", names)}");

            return cachedWowProcess;
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
            GetWindowThreadProcessId(hwnd, out uint pid);
            return Process.GetProcessById((int)pid);
        }

        public static void PressKey(ConsoleKey key)
        {
            SendKeyMessage(key, WM_KEYDOWN);
            Thread.Sleep(50 + random.Next(0, 75));
            SendKeyMessage(key, WM_KEYUP);
        }

        private static void SendKeyMessage(ConsoleKey key, uint message)
        {
            Process? wowProcess = GetWowProcess();
            if (wowProcess != null)
                PostMessage(wowProcess.MainWindowHandle, message, (int)key, 0);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        public static void RightClickMouse(Point position)
        {
            RightClickMouse_LiamCooper(position);
        }

        public static void RightClickMouse_LiamCooper(Point position)
        {
            Process activeProcess = GetActiveProcess();
            Process? wowProcess = GetWowProcess();
            if (wowProcess != null)
            {
                mouse_event((int)MouseEvents.RightUp, position.X, position.Y, 0, 0);
                Point oldPosition = Cursor.Position;

                Thread.Sleep(200);
                Cursor.Position = position;

                Thread.Sleep(LootDelay);
                mouse_event((int)MouseEvents.RightDown, position.X, position.Y, 0, 0);

                Thread.Sleep(30 + random.Next(0, 47));
                mouse_event((int)MouseEvents.RightUp, position.X, position.Y, 0, 0);
                RefocusOnOldScreen(activeProcess, wowProcess, oldPosition);

                Thread.Sleep(LootDelay / 2);
            }
        }

        private static void RefocusOnOldScreen(Process activeProcess, Process wowProcess, Point oldPosition)
        {
            try
            {
                if (activeProcess.MainWindowTitle != wowProcess.MainWindowTitle)
                {
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONDOWN, Keys.VK_RMB, 0);
                    Thread.Sleep(30);
                    PostMessage(activeProcess.MainWindowHandle, Keys.WM_RBUTTONUP, Keys.VK_RMB, 0);

                    SendKeyMessage(ConsoleKey.Escape, WM_KEYDOWN);
                    Thread.Sleep(30);
                    SendKeyMessage(ConsoleKey.Escape, WM_KEYUP);

                    Cursor.Position = oldPosition;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEvents
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
