using System;
using System.Drawing;
using System.Windows.Forms;
using ChatClientApp.Network;

namespace ClientModeratorApp
{
    public partial class ClientForm : Form
    {
        private readonly ChatClient _chatClient;

        public ClientForm(ChatClient chatClient)
        {
            InitializeComponent();
            _chatClient = chatClient;
            _chatClient.MessageReceived += OnMessageReceived;
            this.FormClosing += ClientForm_FormClosing;
        }

        private void OnMessageReceived(string message)
        {
            // Если сообщение начинается с "BUTTON:" – можно добавить отдельную обработку,
            // здесь же для клиента считаем все сообщения текстовыми.
            this.Invoke(new Action(() =>
            {
                AddMessageToChat("Собеседник: " + message);
            }));
        }

        private void AddMessageToChat(string message)
        {
            chatTextBox.AppendText(message + Environment.NewLine);
            chatTextBox.SelectionStart = chatTextBox.Text.Length;
            chatTextBox.ScrollToCaret();
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            string message = inputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await _chatClient.SendMessageAsync(message);
                AddMessageToChat("Я: " + message);
                inputTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Введите сообщение перед отправкой!",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatClient.MessageReceived -= OnMessageReceived;
            _chatClient.Disconnect();
        }
    }
}
