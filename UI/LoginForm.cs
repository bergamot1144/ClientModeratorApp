using System;
using System.Windows.Forms;
using ChatClientApp.Network;

namespace ClientModeratorApp
{
    public partial class LoginForm : Form
    {
        private readonly ChatClient _chatClient;
        private bool _loginProcessed = false;

        public LoginForm(ChatClient chatClient)
        {
            InitializeComponent();
            _chatClient = chatClient;
            _chatClient.MessageReceived += OnLoginResponse;
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            string password = passwordTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }
            try
            {
                string loginMsg = $"LOGIN:{username}:{password}";
                await _chatClient.SendMessageAsync(loginMsg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении: {ex.Message}");
            }
        }

        private void OnLoginResponse(string response)
        {
            if (_loginProcessed)
                return;
            _loginProcessed = true;
            _chatClient.MessageReceived -= OnLoginResponse;

            if (response.StartsWith("LOGIN_OK:Moderator"))
            {
                this.Invoke(new Action(() =>
                {
                    this.Hide();
                    var clientListForm = new ClientListForm(_chatClient);
                    clientListForm.ShowDialog();
                    this.Close();
                }));
            }
            else if (response.StartsWith("LOGIN_OK:Client"))
            {
                this.Invoke(new Action(() =>
                {
                    this.Hide();
                    var clientForm = new ClientForm(_chatClient);
                    clientForm.ShowDialog();
                    this.Close();
                }));
            }
            else if (response.StartsWith("LOGIN_FAIL"))
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Неверные учетные данные.");
                    _loginProcessed = false;
                    _chatClient.MessageReceived += OnLoginResponse;
                }));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show("Неизвестный ответ сервера: " + response);
                    _loginProcessed = false;
                    _chatClient.MessageReceived += OnLoginResponse;
                }));
            }
        }
    }
}
