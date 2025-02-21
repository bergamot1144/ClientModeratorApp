using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatClientApp.Network
{
    public class ChatClient : IDisposable
    {
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        // События для UI
        public event Action<string> MessageReceived;
        public event Action<string[]> ActiveClientsUpdated;

        public ChatClient(string ipAddress, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(ipAddress, port);
                var ns = _tcpClient.GetStream();
                _reader = new StreamReader(ns, Encoding.UTF8);
                _writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
                _cts = new CancellationTokenSource();

                _listenTask = ListenAsync(_cts.Token);

                Console.WriteLine($"[ChatClient] Подключен к серверу {ipAddress}:{port}");
                Console.WriteLine("[ChatClient] Проверка: ListenAsync должен быть запущен!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatClient] Ошибка подключения: {ex.Message}");
            }
        }

        private async Task ListenAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string message = await _reader.ReadLineAsync();

                    if (message == null)
                    {
                        Console.WriteLine("[ChatClient] Сервер закрыл соединение.");
                        break;
                    }

                    Console.WriteLine($"[ChatClient] ListenAsync получил сообщение: '{message}'");
                    ProcessMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[ChatClient] ListenAsync: отменено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatClient] Ошибка в ListenAsync: {ex.Message}");
            }
        }

        private void ProcessMessage(string message)
        {
            Console.WriteLine($"[ChatClient] ProcessMessage вызван с: '{message}'");

            message = message.Trim();

            if (message.StartsWith("CLIENT_LIST:", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[ChatClient] Сообщение распознано как CLIENT_LIST.");

                string clientsStr = message.Substring("CLIENT_LIST:".Length);
                string[] clients = clientsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine($"[ChatClient] Клиенты после разбора: {string.Join(", ", clients)}");

                if (ActiveClientsUpdated != null)
                {
                    Console.WriteLine("[ChatClient] Вызов события ActiveClientsUpdated...");
                    ActiveClientsUpdated.Invoke(clients);
                }
                else
                {
                    Console.WriteLine("[ChatClient] Событие ActiveClientsUpdated НЕ подписано!");
                }
            }
            else
            {
                MessageReceived?.Invoke(message);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
            {
                Console.WriteLine("[ChatClient] Ошибка: нет соединения с сервером.");
                return;
            }
            try
            {
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync(); // Убедимся, что сообщение действительно ушло
                Console.WriteLine($"[ChatClient] Отправлено: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatClient] Ошибка отправки сообщения: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _cts.Cancel();
            try { _tcpClient?.Close(); } catch { }
            Console.WriteLine("[ChatClient] Клиент отключен.");
        }

        public void Dispose()
        {
            Disconnect();
            _cts.Dispose();
        }
    }
}
