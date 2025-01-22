using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClientApp.Network
{
    public class ChatClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public TcpClient TcpClient => _tcpClient;

        public event Action<string> MessageReceived;

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public ChatClient(string ipAddress, int port)
        {
            Connect(ipAddress, port);
        }

        public void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Console.WriteLine("Отправлено: " + message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка отправки сообщения: " + ex.Message);
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_stream.DataAvailable)
                    {
                        bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Получено: " + message);

                        if (message.StartsWith("CLIENT_LIST:"))
                        {
                            HandleClientList(message);
                        }
                        else if (message.StartsWith("CONNECT:"))
                        {
                            HandleConnection(message);
                        }
                        else
                        {
                            MessageReceived?.Invoke(message);
                        }
                    }
                    else
                    {
                        await Task.Delay(100, token); // Даем процессору отдохнуть
                    }
                }
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Ошибка получения сообщения: " + ex.Message);
                }
            }
        }


        public event Action<string[]> ClientListUpdated;
        private void HandleClientList(string message)
        {
            string[] clients = message.Substring("CLIENT_LIST:".Length).Split(',');
            Console.WriteLine("Список клиентов:");
            foreach (var client in clients)
            {
                Console.WriteLine(client);
            }
            ClientListUpdated?.Invoke(clients); // Вызываем событие
        }

        private void HandleConnection(string message)
        {
            string[] parts = message.Split(':');
            if (parts.Length == 2)
            {
                string targetClientId = parts[1];
                Console.WriteLine($"Вы подключены к клиенту {targetClientId}");
            }
        }
        public void UpdateClientList(string[] clientIds, ListBox clientbox)
        {
            clientbox.Items.Clear();

            if (clientIds.Length == 0)
            {
                MessageBox.Show("Нет доступных клиентов.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                foreach (var clientId in clientIds)
                {
                    clientbox.Items.Add(clientId);
                }
            }
        }


        public void Connect(string ipAddress, int port)
        {
            try
            {
                Console.WriteLine("Попытка подключиться к серверу...");

                _tcpClient = new TcpClient();
                _tcpClient.Connect(ipAddress, port);
                Console.WriteLine($"Подключен к серверу {ipAddress}:{port}");
                _stream = _tcpClient.GetStream();

                _cts = new CancellationTokenSource();
                _listenTask = ListenForMessagesAsync(_cts.Token);
            }
            catch (SocketException se)
            {
                Console.WriteLine("Ошибка сокета при подключении: " + se.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка подключения: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (_tcpClient != null && _tcpClient.Connected)
            {
                _cts?.Cancel(); // Останавливаем поток
                _listenTask?.Wait(); // Ожидаем завершения задачи

                _stream.Close();
                _tcpClient.Close();
                Console.WriteLine("Клиент отключен.");
            }
        }
    }
}
