using System;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientModeratorApp
{
    public partial class ModeratorForm : Form
    {
        private TcpClient _tcpClient;        // Сокет для взаимодействия
        private NetworkStream _stream;       // Поток для чтения/записи
        private Thread _listenThread;        // Поток, где читаем входящие сообщения
        private volatile bool _shouldStop;   // Флаг для завершения чтения

        private string _targetClientName;    // Просто отображаем, с кем общаемся

        public ModeratorForm(TcpClient tcpClient, string targetClientName)
        {
            InitializeComponent();

            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();    // Получаем основной поток
            _targetClientName = targetClientName;

            // Показываем, с кем общаемся
            this.Text = $"Moderator Chat with {_targetClientName}";

            // При запуске все кнопки OFF
            SetAllButtonsOff();

            // Запускаем фоновый поток чтения
            StartListening();

            // При закрытии формы завершаем поток
            this.FormClosing += ModeratorForm_FormClosing;
        }

        // Устанавливаем все кнопки в "OFF" (серый цвет)
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

        // Запускаем фоновый поток для чтения сообщений
        private void StartListening()
        {
            _listenThread = new Thread(ListenForMessages);
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        // Бесконечное чтение _stream.Read(...) пока не закроем форму
        private void ListenForMessages()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (!_shouldStop)
                {
                    // Читаем из потока
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break; // Сервер разорвал соединение

                    // Декодируем сообщение в строку
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("[ModeratorForm] Получено: " + message);

                    // Проверяем, начинается ли сообщение с "BUTTON:"
                    if (message.StartsWith("BUTTON:"))
                    {
                        // Это "кнопочное" сообщение (например, "BUTTON:1:ON")
                        if (InvokeRequired)
                        {
                            // Если поток не UI, делаем Invoke
                            Invoke(new Action<string>(HandleButtonMessage), message);
                        }
                        else
                        {
                            HandleButtonMessage(message);
                        }
                    }
                    else
                    {
                        // Любое другое сообщение считаем "текстовым"
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                AddMessageToChat("Собеседник: " + message);
                            }));
                        }
                        else
                        {
                            AddMessageToChat("Собеседник: " + message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Если мы сами не устанавливали _shouldStop в true,
                // логируем ошибку чтения
                if (!_shouldStop)
                {
                    Console.WriteLine("[ModeratorForm] Ошибка чтения: " + ex.Message);
                }
            }
        }


        // Обработка входящего "BUTTON:номер:ON/OFF" 
        private void HandleButtonMessage(string buttonMsg)
        {
            // Пример: "BUTTON:1:ON"
            // Можно просто вывести в чат
            chatTextBox.AppendText("Клиент нажал: " + buttonMsg + Environment.NewLine);

            // Либо, если хотите менять кнопки у модератора,
            // сделайте логику вроде: UpdateButtonState(buttonMsg);
        }

        // Метод для добавления текста в чат
        private void AddMessageToChat(string message)
        {
            string formattedMessage = ">>>>: " + message + Environment.NewLine;
            chatTextBox.AppendText(formattedMessage);
            chatTextBox.SelectionStart = chatTextBox.Text.Length;
            chatTextBox.ScrollToCaret();
        }

        // Отправка текстового сообщения
        private void SendMessage()
        {
            string message = inputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _stream.Write(data, 0, data.Length);

                    AddMessageToChat("Я: " + message);
                    inputTextBox.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке сообщения: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
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

        private void sendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                // e.Handled = true;
            }
        }

        // Обработка нажатий на кнопки 1..8 (переключение ON/OFF)
        private void ToggleButtonState(object sender, int buttonNumber)
        {
            var button = sender as Button;
            bool isOn = (button.BackColor == Color.Gray);
            button.BackColor = isOn ? Color.Green : Color.Gray;

            // Отправим состояние на сервер
            SendButtonState(buttonNumber, isOn);
        }

        private void button1_Click(object sender, EventArgs e) { ToggleButtonState(sender, 1); }
        private void button2_Click(object sender, EventArgs e) { ToggleButtonState(sender, 2); }
        private void button3_Click(object sender, EventArgs e) { ToggleButtonState(sender, 3); }
        private void button4_Click(object sender, EventArgs e) { ToggleButtonState(sender, 4); }
        private void button5_Click(object sender, EventArgs e) { ToggleButtonState(sender, 5); }
        private void button6_Click(object sender, EventArgs e) { ToggleButtonState(sender, 6); }
        private void button7_Click(object sender, EventArgs e) { ToggleButtonState(sender, 7); }
        private void button8_Click(object sender, EventArgs e) { ToggleButtonState(sender, 8); }

        private void SendButtonState(int buttonNumber, bool isOn)
        {
            string state = isOn ? "ON" : "OFF";
            string message = $"BUTTON:{buttonNumber}:{state}";

            // Пишем напрямую в поток
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);

                // Можно локально отобразить
                chatTextBox.AppendText($"Я нажал кнопку {buttonNumber} -> {state}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке состояния кнопки: " + ex.Message);
            }
        }

        private void ModeratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти из чата?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                _shouldStop = true;
                try
                {
                    _stream.Close();
                    _tcpClient.Close();
                }
                catch { }

                if (_listenThread != null && _listenThread.IsAlive)
                {
                    _listenThread.Abort();
                }
            }
        }
    }
}