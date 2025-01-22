using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ClientModeratorApp
{
    public partial class ClientListForm : Form
    {
        private TcpClient _tcpClient;          // Сокет модератора
        private NetworkStream _stream;         // Поток
        private Thread _listenThread;          // Фоновый поток чтения
        private volatile bool _shouldStop = false; // флаг для выхода из цикла чтения

        public ClientListForm(TcpClient tcpClient)
        {
            InitializeComponent();

            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();

            // Подписка на FormClosing, чтобы корректно закрыть поток
            this.FormClosing += ClientListForm_FormClosing;

            // Запускаем «старый» подход чтения
            StartListening();
        }

        // Запуск фонового потока, который читает _stream.Read(...)
        private void StartListening()
        {
            _listenThread = new Thread(ListenForServerMessages);
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        // Бесконечное чтение сообщений от сервера
        private void ListenForServerMessages()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (!_shouldStop)
                {
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Сервер закрыл соединение

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("[ClientListForm] Получено от сервера: " + message);

                    if (message.StartsWith("CLIENT_LIST:"))
                    {
                        // "CLIENT_LIST:c123,Gucci"
                        string clientList = message.Substring("CLIENT_LIST:".Length);
                        string[] clientIds = clientList.Split(',');
                        HandleClientListMessage(clientIds);
                    }
                    else if (message.StartsWith("CONNECT_OK:"))
                    {
                        // "CONNECT_OK:c123"
                        string targetClientName = message.Substring("CONNECT_OK:".Length);
                        OpenModeratorForm(targetClientName);
                    }
                    else
                    {
                        // Прочие сообщения
                        Console.WriteLine("[ClientListForm] Необработанное сообщение: " + message);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_shouldStop)
                {
                    Console.WriteLine("[ClientListForm] Ошибка чтения: " + ex.Message);
                }
            }
        }

        // Метод, обновляющий список клиентов в ListBox
        private void HandleClientListMessage(string[] clientIds)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string[]>(HandleClientListMessage), clientIds);
                return;
            }
            clientbox.Items.Clear();
            if (clientIds.Length == 0)
            {
                MessageBox.Show("Нет доступных клиентов.");
            }
            else
            {
                foreach (string client in clientIds)
                {
                    if (!string.IsNullOrEmpty(client))
                        clientbox.Items.Add(client);
                }
            }
        }

        // Открытие ModeratorForm
        private void OpenModeratorForm(string targetClientName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OpenModeratorForm), targetClientName);
                return;
            }
            // Открываем форму модератора, передав тот же TcpClient (тот же поток)
            // и имя клиента, к которому модератор подключился
            ModeratorForm modForm = new ModeratorForm(_tcpClient, targetClientName);
            modForm.Show();
        }

        // Нажатие кнопки "Connect"
        private void button_connect_Click(object sender, EventArgs e)
        {
            if (clientbox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для подключения.");
                return;
            }

            string selectedClient = clientbox.SelectedItem.ToString();

            // Отправляем "CONNECT:c123"
            string message = $"CONNECT:{selectedClient}";
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.Write(data, 0, data.Length);

            MessageBox.Show($"Запрошено подключение к {selectedClient}");
        }

        // Нажатие кнопки Refresh - запрашиваем список клиентов
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            string request = "CLIENT_LIST";
            byte[] data = Encoding.UTF8.GetBytes(request);
            _stream.Write(data, 0, data.Length);

            Console.WriteLine("[ClientListForm] Отправлен запрос CLIENT_LIST на сервер.");
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            clientbox.Items.Clear();
            clientbox.Items.Add("TestUser1");
            clientbox.Items.Add("TestUser2");
            clientbox.Items.Add("TestUser3");
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close(); // Вызывает FormClosing
        }

        // Событие FormClosing для корректного завершения
        private void ClientListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?",
                                         "Подтверждение выхода",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                // Останавливаем поток
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

                Application.Exit(); // Выходим из приложения
            }
        }
    }
}
