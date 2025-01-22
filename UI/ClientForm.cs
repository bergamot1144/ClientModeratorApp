using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientModeratorApp
{
    public partial class ClientForm : Form
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _listenThread;
        private volatile bool _shouldStop = false;

        public ClientForm(TcpClient tcpClient)
        {
            InitializeComponent(); // Важно: инициализация компонентов формы

            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();

            SetAllButtonsOff();
            StartListening();

            this.FormClosing += ClientForm_FormClosing;
        }

        // Метод для установки всех кнопок в "OFF" (серый цвет)
        private void SetAllButtonsOff()
        {
            button1.BackColor = Color.Gray;
            button2.BackColor = Color.Gray;
            button3.BackColor = Color.Gray;
            button4.BackColor = Color.Gray;
            button5.BackColor = Color.Gray;
            button6.BackColor = Color.Gray;
            button7.BackColor = Color.Gray;
            button8.BackColor = Color.Gray;
        }

        // Запуск фонового потока для чтения сообщений
        private void StartListening()
        {
            _listenThread = new Thread(ListenForMessages);
            _listenThread.IsBackground = true;
            _listenThread.Start();
            Console.WriteLine("[DEBUG] (ClientForm) Listening thread started.");
        }

        // Метод для чтения сообщений от сервера
        private void ListenForMessages()
        {
            Console.WriteLine("[DEBUG] (ClientForm) ListenForMessages: Start loop");
            byte[] buffer = new byte[1024];
            try
            {
                while (!_shouldStop)
                {
                    // Чтение из потока
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"[DEBUG] (ClientForm) bytesRead = {bytesRead}");

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("[DEBUG] (ClientForm) Server closed connection, breaking loop.");
                        break; // Сервер закрыл соединение
                    }

                    // Преобразование байтов в строку
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[DEBUG] (ClientForm) Raw message received: '{msg}'");

                    // Проверка, начинается ли сообщение с "BUTTON:"
                    if (msg.StartsWith("BUTTON:"))
                    {
                        Console.WriteLine("[DEBUG] (ClientForm) Processing BUTTON message.");
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                UpdateButtonState(msg);
                                Console.WriteLine("[DEBUG] (ClientForm) BUTTON state updated via Invoke.");
                            }));
                        }
                        else
                        {
                            UpdateButtonState(msg);
                            Console.WriteLine("[DEBUG] (ClientForm) BUTTON state updated directly.");
                        }
                    }
                    else
                    {
                        // Обычное текстовое сообщение
                        Console.WriteLine("[DEBUG] (ClientForm) Processing TEXT message.");
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                AddMessageToChat("Собеседник: " + msg);
                                Console.WriteLine("[DEBUG] (ClientForm) TEXT message displayed via Invoke.");
                            }));
                        }
                        else
                        {
                            AddMessageToChat("Собеседник: " + msg);
                            Console.WriteLine("[DEBUG] (ClientForm) TEXT message displayed directly.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_shouldStop)
                {
                    Console.WriteLine("[DEBUG] (ClientForm) Exception in ListenForMessages: " + ex.Message);
                }
            }
            Console.WriteLine("[DEBUG] (ClientForm) ListenForMessages: Loop ended.");
        }

        // Метод для обновления состояния кнопок на форме
        private void UpdateButtonState(string message)
        {
            // Ожидаемый формат: "BUTTON:1:ON" или "BUTTON:1:OFF"
            string[] parts = message.Split(':');
            if (parts.Length == 3 && parts[0] == "BUTTON")
            {
                if (int.TryParse(parts[1], out int buttonNumber))
                {
                    bool isOn = (parts[2] == "ON");

                    // Поиск кнопки по имени (button1, button2 и т.д.)
                    Button button = this.Controls.Find($"button{buttonNumber}", true).FirstOrDefault() as Button;
                    if (button != null)
                    {
                        button.BackColor = isOn ? Color.Green : Color.Gray;
                        Console.WriteLine($"[DEBUG] (ClientForm) Button{buttonNumber} set to {(isOn ? "ON" : "OFF")}.");
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] (ClientForm) Button{buttonNumber} not found.");
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] (ClientForm) Invalid button number: {parts[1]}");
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG] (ClientForm) Invalid BUTTON message format: {message}");
            }
        }

        // Метод для добавления сообщений в чат
        private void AddMessageToChat(string message)
        {
            //MessageBox.Show($"DEBUG: AddMessageToChat called with message: '{message}'");

            if (chatTextBox == null)
            {
                MessageBox.Show("DEBUG: chatTextBox is null!");
                return;
            }

            chatTextBox.AppendText(message + Environment.NewLine);
            chatTextBox.SelectionStart = chatTextBox.Text.Length;
            chatTextBox.ScrollToCaret();
            Debug.WriteLine("[DEBUG] (ClientForm) Message appended to chatTextBox.");
        }


        // Обработчик нажатия кнопки "SEND"
        private void sendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // Обработчик нажатия клавиши Enter в inputTextBox
        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                e.Handled = true; // Опционально: предотвращает добавление новой строки в TextBox
                e.SuppressKeyPress = true; // Опционально: предотвращает звук при нажатии Enter
            }
        }

        // Метод для отправки сообщений на сервер
        private void SendMessage()
        {
            string message = inputTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _stream.Write(data, 0, data.Length);
                    Console.WriteLine($"[DEBUG] (ClientForm) Sent message: '{message}'");

                    AddMessageToChat("Я: " + message);
                    inputTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке сообщения: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    Console.WriteLine($"[DEBUG] (ClientForm) Exception while sending message: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Введите сообщение перед отправкой!",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        // Обработчик закрытия формы
        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из чата?",
                                         "Подтверждение выхода",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true; // Отменяет закрытие формы
            }
            else
            {
                _shouldStop = true; // Останавливаем цикл чтения
                try
                {
                    _stream.Close();
                    _tcpClient.Close();
                    Console.WriteLine("[DEBUG] (ClientForm) Connection closed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] (ClientForm) Exception while closing connection: {ex.Message}");
                }

                if (_listenThread != null && _listenThread.IsAlive)
                {
                    _listenThread.Abort(); // Прерываем поток
                    Console.WriteLine("[DEBUG] (ClientForm) Listening thread aborted.");
                }
            }
        }
    }
}