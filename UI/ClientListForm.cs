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
            _chatClient.RoomListUpdated += OnRoomListUpdated;
            Console.WriteLine("[ClientListForm] Подписка на RoomListUpdated выполнена.");

            // Если до получения данных список пустой, сразу устанавливаем значение по умолчанию:
            UpdateClientBox(null);
        }


        private void OnRoomListUpdated(string[] rooms)
        {
            Console.WriteLine("[ClientListForm] Событие RoomListUpdated вызвано.");
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateClientBox(rooms)));
            }
            else
            {
                UpdateClientBox(rooms);
            }
        }


        private void UpdateClientBox(string[] items)
        {
            clientbox.Items.Clear();
            if (items != null && items.Length > 0)
            {
                clientbox.Items.AddRange(items);
                Console.WriteLine("[ClientListForm] ListBox updated with: " + string.Join(", ", items));
            }
            else
            {
                clientbox.Items.Add("Нет активных клиентов.");
                Console.WriteLine("[ClientListForm] ListBox updated with default message: Нет активных клиентов.");
            }
        }


        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            Console.WriteLine("[ClientListForm] Нажата кнопка Refresh. Отправка команды CLIENT_LIST.");
            await _chatClient.SendMessageAsync("CLIENT_LIST");
        }

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
            _chatClient.RoomListUpdated -= OnRoomListUpdated;
            _chatClient.Disconnect();
            Console.WriteLine("[ClientListForm] Форма закрывается. Отписка и отключение выполнены.");
        }
    }
}
