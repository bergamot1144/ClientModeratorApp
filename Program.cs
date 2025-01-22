using System;
using System.Windows.Forms;
using ChatClientApp.Network; // Пространство имен для ChatClient

namespace ClientModeratorApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ChatClient chatClient = new ChatClient("127.0.0.1", 9000); // Подключаемся сразу
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm()); // Передаём клиента в LoginForm

        }
    }
}
