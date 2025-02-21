using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ChatClientApp.Network;

namespace ClientModeratorApp
{
    public partial class ClientListForm : Form
    {
        private readonly ChatClient _chatClient;

        public ClientListForm(ChatClient chatClient)
        {
            InitializeComponent();
            _chatClient = chatClient;

            // Подписка на обновление списка клиентов
            _chatClient.ActiveClientsUpdated += OnActiveClientsUpdated;
            Console.WriteLine("[ClientListForm] Подписка на ActiveClientsUpdated выполнена.");
        }

        // Обработчик события обновления списка клиентов
        private void OnActiveClientsUpdated(string[] clients)
        {
            Console.WriteLine("[ClientListForm] Событие ActiveClientsUpdated получено."); // <=== ОТЛАДКА
            if (clientbox.InvokeRequired)
            {
                Console.WriteLine($"[ClientListForm] Выполняется UpdateClientBox, клиентов: {clients.Length}");


                clientbox.Invoke(new Action(() => UpdateClientBox(clients)));
            }
            else
            {
                Console.WriteLine($"[ClientListForm] Выполняется UpdateClientBox, клиентов: {clients.Length}");

                UpdateClientBox(clients);
            }
        }

        // Метод обновления списка клиентов в UI
        private void UpdateClientBox(string[] clients)
        {
            clientbox.Items.Clear();
            Console.WriteLine("[ClientListForm] Обновление списка клиентов..."); // <=== ОТЛАДКА
            clientbox.Items.Add("TestClient1");
            clientbox.Items.Add("TestClient2");

            Console.WriteLine($"[ClientListForm] Список клиентов перед добавлением в clientbox: {string.Join(", ", clients)}");

            // Если список пуст, показываем сообщение о том, что нет активных клиентов
            if (clients.Length == 0 || (clients.Length == 1 && string.IsNullOrEmpty(clients[0])))
            {
                clientbox.Items.Add("Нет активных клиентов");
            }
            else
            {
                clientbox.Items.AddRange(clients);
            }
        }

        // Метод для кнопки Refresh
        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[ClientListForm] Нажата кнопка Refresh. Отправка команды CLIENT_LIST.");
            await _chatClient.SendMessageAsync("CLIENT_LIST");
        }

        // Метод для кнопки Connect
        private void button_connect_Click(object sender, EventArgs e)
        {
            if (clientbox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для подключения.");
                return;
            }
            string selectedClient = clientbox.SelectedItem.ToString();
            Console.WriteLine($"[ClientListForm] Нажата кнопка Connect. Выбран клиент: {selectedClient}");

            // Дожидаемся отправки сообщения перед выходом
            _chatClient.SendMessageAsync("CONNECT:" + selectedClient).Wait();
        }

        // Тестовый метод для заполнения списка
        private void buttonTest_Click(object sender, EventArgs e)
        {
            clientbox.Items.Clear();
            clientbox.Items.Add("TestClient1");
            clientbox.Items.Add("TestClient2");
            clientbox.Items.Add("TestClient3");
            Console.WriteLine("[ClientListForm] Нажата кнопка Test. ListBox заполнен тестовыми значениями.");
        }

        // Метод для закрытия формы и отписки от события
        private void ClientListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatClient.ActiveClientsUpdated -= OnActiveClientsUpdated;
            _chatClient.Disconnect();
            Console.WriteLine("[ClientListForm] Форма закрывается. Отписка и отключение выполнены.");
        }

        private void ClientListForm_Load(object sender, EventArgs e)
        {

        }
    }
}
