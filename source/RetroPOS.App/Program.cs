using System;
using System.Runtime.InteropServices;
using Terminal.Gui;

namespace RetroPOS.App
{
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;

        public static Action running = MainApp;

        static void Main()
        {
            while (running != null)
            {
                running.Invoke();
            }
            Application.Shutdown();
        }

        public static void MainApp()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            ShowWindow(ThisConsole, MAXIMIZE);

            Application.Init();
            var top = Application.Top;

            var win = new Window("Welcome to RetroPOS")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Search Products", "", () => {
                        Action action = () => SearchProductsWindow.Draw();
                        running = action;
                        Application.RequestStop();
                    }),
                    null,
                    new MenuItem ("_Exit", "", () => { if (Quit ()) { running = null; top.Running = false; } })
                })
            });
            top.Add(menu);

            var intro = new Label("DISCLAIMER: This application is designed to foster learning via implementing Cloud Native practices in Azure using DAPR and .NET Core Microservices.") { X = 3, Y = 2 };
            win.Add(intro);

            Application.Run();
        }

        static bool Quit()
        {
            var n = MessageBox.Query(50, 7, "Quit RetroPOS", "Are you sure you want to quit this application?", "Yes", "No");
            return n == 0;
        }
    }
}
