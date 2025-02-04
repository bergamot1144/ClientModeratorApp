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
        }

        private void OnRoomListUpdated(string[] rooms)
        {
            Console.WriteLine("[ClientListForm] Событие RoomListUpdated вызвано.");
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateRoomBox(rooms)));
            }
            else
            {
                UpdateRoomBox(rooms);
            }
        }

        private void UpdateRoomBox(string[] rooms)
        {
            clientbox.Items.Clear();
            if (rooms != null && rooms.Length > 0)
            {
                clientbox.Items.AddRange(rooms);
                Console.WriteLine("[ClientListForm] clientbox обновлён: " + string.Join(", ", rooms));

                // Добавляем окно с информацией, чтобы увидеть, что метод UpdateRoomBox был вызван.
                MessageBox.Show("UpdateRoomBox called: " + string.Join(", ", rooms),
                                "DEBUG: ClientListForm.UpdateRoomBox",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                clientbox.Items.Add("Нет доступных комнат.");
                Console.WriteLine("[ClientListForm] clientbox обновлён: список пуст.");

                MessageBox.Show("UpdateRoomBox called: empty list",
                                "DEBUG: ClientListForm.UpdateRoomBox",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
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
                MessageBox.Show("Пожалуйста, выберите комнату для подключения.");
                return;
            }
            string selectedRoom = clientbox.SelectedItem.ToString();
            Console.WriteLine("[ClientListForm] Нажата кнопка Connect. Выбрана комната: " + selectedRoom);
            _chatClient.SendMessageAsync("CONNECT:" + selectedRoom);
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            clientbox.Items.Clear();
            clientbox.Items.Add("TestRoom1");
            clientbox.Items.Add("TestRoom2");
            clientbox.Items.Add("TestRoom3");
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
