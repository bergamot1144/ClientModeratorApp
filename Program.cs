﻿using System;
using System.Text;
using System.Windows.Forms;
using ChatClientApp.Network;

namespace ClientModeratorApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            ChatClient chatClient = new ChatClient("127.0.0.1", 9000);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm(chatClient));
        }
    }
}
