using System;
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
            // Подписываемся на событие обновления списка активных клиентов
            _chatClient.ActiveClientsUpdated += OnActiveClientsUpdated;
            Console.WriteLine("[ClientListForm] Подписка на ActiveClientsUpdated выполнена.");
        }

        private void OnActiveClientsUpdated(string[] clients)
        {
            Console.WriteLine("[ClientListForm] Событие ActiveClientsUpdated вызвано.");
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateClientBox(clients)));
            }
            else
            {
                UpdateClientBox(clients);
            }
        }

        private void UpdateClientBox(string[] clients)
        {
            clientbox.Items.Clear();
            if (clients != null && clients.Length > 0)
            {
                clientbox.Items.AddRange(clients);
                Console.WriteLine("[ClientListForm] ListBox обновлён: " + string.Join(", ", clients));
            }
            else
            {
                clientbox.Items.Add("Нет активных клиентов.");
                Console.WriteLine("[ClientListForm] ListBox обновлён: список пуст.");
            }
        }

        // Обработчик кнопки Refresh для принудительного запроса списка активных клиентов.
        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[ClientListForm] Нажата кнопка Refresh. Отправка команды CLIENT_LIST.");
            await _chatClient.SendMessageAsync("CLIENT_LIST");
        }

        // Обработчик кнопки Connect для подключения модератора к выбранному клиенту.
        private void button_connect_Click(object sender, EventArgs e)
        {
            if (clientbox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для подключения.");
                return;
            }
            string selectedClient = clientbox.SelectedItem.ToString();
            Console.WriteLine("[ClientListForm] Нажата кнопка Connect. Выбран клиент: " + selectedClient);
            _chatClient.SendMessageAsync("CONNECT:" + selectedClient);
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            clientbox.Items.Clear();
            clientbox.Items.Add("TestClient1");
            clientbox.Items.Add("TestClient2");
            clientbox.Items.Add("TestClient3");
            Console.WriteLine("[ClientListForm] Нажата кнопка Test. ListBox заполнен тестовыми значениями.");
        }

        private void ClientListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatClient.ActiveClientsUpdated -= OnActiveClientsUpdated;
            _chatClient.Disconnect();
            Console.WriteLine("[ClientListForm] Форма закрывается. Отписка и отключение выполнены.");
        }
    }
}
