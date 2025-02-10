using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClientApp.Network
{
    /// <summary>
    /// Класс для сетевого взаимодействия. Он устанавливает соединение с сервером,
    /// читает строки через ReadLineAsync и генерирует события для форм.
    /// </summary>
    public class ChatClient : IDisposable
    {
        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cts;
        private Task _listenTask;

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        // События для передачи сообщений в UI
        public event Action<string> MessageReceived;
        // Событие для обновления списка активных клиентов (от сервера отправляется "CLIENT_LIST:...")
        public event Action<string[]> ActiveClientsUpdated;
        // Если сервер использует другой префикс, например "ROOM_LIST:", можно добавить событие RoomListUpdated,
        // но в этом решении сервер отправляет "CLIENT_LIST:" для активных клиентов.

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
                    Console.WriteLine("[ChatClient] ListenAsync: ожидаем ReadLineAsync...");
                    string line = await _reader.ReadLineAsync();
                    if (line == null)
                    {
                        Console.WriteLine("[ChatClient] ListenAsync: прочитано null — сервер закрыл соединение.");
                        break;
                    }
                    Console.WriteLine("[ChatClient] ListenAsync: прочитана строка: " + line);
                    ProcessMessage(line);
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
            message = message.Trim();
            Console.WriteLine($"[ChatClient] ProcessMessage: '{message}'");

            if (message.StartsWith("CLIENT_LIST:", StringComparison.OrdinalIgnoreCase))
            {
                string clientsStr = message.Substring("CLIENT_LIST:".Length);
                string[] clients = clientsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i] = clients[i].Trim();
                }
                Console.WriteLine($"[ChatClient] Получен список активных клиентов: {string.Join(" | ", clients)}");
                ActiveClientsUpdated?.Invoke(clients);
            }
            else
            {
                MessageReceived?.Invoke(message);
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
                return;
            try
            {
                await _writer.WriteLineAsync(message);
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
            try { _tcpClient.Close(); } catch { }
            Console.WriteLine("[ChatClient] Клиент отключен.");
        }

        public void Dispose()
        {
            Disconnect();
            _cts.Dispose();
        }
    }
}
