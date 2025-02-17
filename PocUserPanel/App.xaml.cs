﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;

namespace ModernDashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }

    class Program
    {
        [DllImport("PocUserDll.dll")]
        static extern int PocUserInitCommPort(ref IntPtr hPort);
        [DllImport("PocUserDll.dll")]
        static extern int PocUserGetMessage(IntPtr hPort, ref uint Command);
        [DllImport("PocUserDll.dll")]
        static extern int PocUserGetMessageEx(IntPtr hPort, ref uint Command, StringBuilder MessageBuffer);
        [DllImport("PocUserDll.dll")]
        static extern int PocUserSendMessage(IntPtr hPort, string Buffer, int Command);
        [DllImport("PocUserDll.dll")]
        static extern int PocUserAddProcessRules(IntPtr hPort, string ProcessName, uint Access);
        [DllImport("Kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        const uint STATUS_SUCCESS = 0x00000001;

        const int POC_PR_ACCESS_READWRITE = 0x00000001;
        const int POC_PR_ACCESS_BACKUP = 0x00000002;

        const int POC_PRIVILEGE_DECRYPT = 0x00000004;
        const int POC_PRIVILEGE_ENCRYPT = 0x00000008;

        const int POC_ADD_SECURE_FODER = 0x00000002;
        const int POC_GET_FILE_EXTENSION = 0x00000006;
        const int POC_REMOVE_SECURE_FOLDER = 0x0000000B;
        const int POC_GET_SECURE_FOLDER = 0x0000000A;

        const int POC_PROCESS_RULES_SIZE = 324;
        const int POC_FILE_EXTENSION_SIZE = 32;
        const int POC_SECURE_FOLDER_SIZE = 320;

        private static IntPtr hPort;

        private static void GetMessageThread()
        {
            uint ReturnCommand = 0;
            PocUserGetMessage(hPort, ref ReturnCommand);

            if (POC_PRIVILEGE_DECRYPT == ReturnCommand)
            {
                //App.Current.Dispatcher.Invoke((Action)(() =>
                //{
                //    ListBox.Items.Add(FolderName.Text);
                //}));

                MessageBox.Show("Poc decrypt file success.");
            }
            else if (POC_PRIVILEGE_ENCRYPT == ReturnCommand)
            {
                MessageBox.Show("Poc encrypt file success.");
            }
            else
            {
                string ErrorText = "Poc failed!->ReturnCommand = %d";
                ErrorText = ErrorText + ReturnCommand.ToString("X");
                MessageBox.Show(ErrorText);

                if (0 != hPort.ToInt32())
                {
                    //MessageBox.Show("hPort close.");
                    CloseHandle(hPort);
                    hPort = (IntPtr)0;
                }

                return;
            }

            if (0 != hPort.ToInt32())
            {
                //MessageBox.Show("hPort close.");
                CloseHandle(hPort);
                hPort = (IntPtr)0;
            }
        }



        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length > 1)
            {
                if (0 == hPort.ToInt32())
                {
                    //MessageBox.Show("New port init.");
                    int ret = PocUserInitCommPort(ref hPort);
                    if (0 != ret)
                    {
                        return;
                    }
                }

                Thread thread = new Thread(new ThreadStart(GetMessageThread));
                thread.Start();

                PocUserSendMessage(hPort, args[1], Convert.ToInt32(args[0]));
            }

            ModernDashboard.App app = new ModernDashboard.App();
            app.InitializeComponent();
            app.Run();
            
        }
    }
}
