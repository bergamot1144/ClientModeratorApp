using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ClientModeratorApp
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string enteredUsername = usernameTextBox.Text.Trim();
            string enteredPassword = passwordTextBox.Text.Trim();

            if (string.IsNullOrEmpty(enteredUsername) || string.IsNullOrEmpty(enteredPassword))
            {
                MessageBox.Show("Введите логин и пароль.",
                                "Ошибка входа",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Подключаемся к серверу
                TcpClient tcpClient = new TcpClient("127.0.0.1", 9000); // IP:port подставьте свои
                NetworkStream stream = tcpClient.GetStream();

                // 2. Отправляем "LOGIN:username:password"
                string loginMsg = $"LOGIN:{enteredUsername}:{enteredPassword}";
                byte[] dataToSend = Encoding.UTF8.GetBytes(loginMsg);
                stream.Write(dataToSend, 0, dataToSend.Length);

                // 3. Читаем ответ (например, "LOGIN_OK:Client" или "LOGIN_OK:Moderator" или "LOGIN_FAIL")
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // Если сервер закрыл соединение
                    MessageBox.Show("Сервер закрыл соединение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("[LoginForm] Ответ сервера: " + response);

                // 4. Разбираем ответ
                if (response.StartsWith("LOGIN_OK:Client"))
                {
                    // Открываем форму клиента (где тоже старый подход)
                    // Передаём tcpClient (та же связь), 
                    // чтобы ClientForm мог сам читать/писать поток
                    var clientForm = new ClientForm(tcpClient);
                    OpenNextForm(clientForm);
                }
                else if (response.StartsWith("LOGIN_OK:Moderator"))
                {
                    // Открываем форму списка клиентов (или ModeratorForm, смотря на логику)
                    var clientListForm = new ClientListForm(tcpClient);
                    OpenNextForm(clientListForm);
                }
                else if (response == "LOGIN_FAIL")
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.",
                                    "Ошибка входа",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    // Закрываем соединение, если не нужно оставлять
                    stream.Close();
                    tcpClient.Close();
                }
                else
                {
                    // Любая другая строка 
                    MessageBox.Show("Неизвестный ответ сервера: " + response,
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    // Можно тоже закрыть соединение
                    stream.Close();
                    tcpClient.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении: {ex.Message}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Открывает другую форму (модально), скрывая текущую.
        /// </summary>
        private void OpenNextForm(Form nextForm)
        {
            this.Hide();
            nextForm.ShowDialog();
            this.Close();
        }
    }
}
