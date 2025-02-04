using System;
using System.Drawing;
using System.Windows.Forms;
using ChatClientApp.Network;

namespace ClientModeratorApp
{
    public partial class ModeratorForm : Form
    {
        private readonly ChatClient _chatClient;
        private readonly string _targetClientName;

        public ModeratorForm(ChatClient chatClient, string targetClientName)
        {
            InitializeComponent();
            _chatClient = chatClient;
            _targetClientName = targetClientName;
            this.Text = $"Moderator Chat with {_targetClientName}";

            // При запуске устанавливаем все кнопки в состояние OFF (серый цвет)
            SetAllButtonsOff();

            // Подписываемся на событие получения сообщений от сервера
            _chatClient.MessageReceived += OnMessageReceived;

            // Подписываемся на событие закрытия формы для корректного завершения работы
            this.FormClosing += ModeratorForm_FormClosing;
        }

        /// <summary>
        /// Обработчик входящих сообщений.
        /// Если сообщение начинается с "BUTTON:" – обновляем состояние кнопок,
        /// иначе отображаем как текстовое сообщение.
        /// </summary>
        private void OnMessageReceived(string message)
        {
            if (message.StartsWith("BUTTON:"))
            {
                // Обработка сообщения изменения состояния кнопок
                this.Invoke(new Action(() =>
                {
                    UpdateButtonState(message);
                    AddMessageToChat("Клиент: " + message);
                }));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    AddMessageToChat("Собеседник: " + message);
                }));
            }
        }

        /// <summary>
        /// Обновляет состояние кнопки на форме.
        /// Ожидается формат: "BUTTON:номер:ON" или "BUTTON:номер:OFF".
        /// </summary>
        private void UpdateButtonState(string message)
        {
            string[] parts = message.Split(':');
            if (parts.Length == 3 && parts[0] == "BUTTON" && int.TryParse(parts[1], out int buttonNumber))
            {
                bool isOn = parts[2].Equals("ON", StringComparison.OrdinalIgnoreCase);
                // Поиск кнопки по имени, например, button1, button2, ...
                Control[] controls = this.Controls.Find($"button{buttonNumber}", true);
                if (controls.Length > 0 && controls[0] is Button btn)
                {
                    btn.BackColor = isOn ? Color.Green : Color.Gray;
                }
            }
        }

        /// <summary>
        /// Устанавливает все кнопки (button1..button8) в состояние OFF (серый цвет).
        /// </summary>
        private void SetAllButtonsOff()
        {
            for (int i = 1; i <= 8; i++)
            {
                Control[] controls = this.Controls.Find($"button{i}", true);
                if (controls.Length > 0 && controls[0] is Button btn)
                {
                    btn.BackColor = Color.Gray;
                }
            }
        }

        /// <summary>
        /// Добавляет сообщение в текстовое поле чата.
        /// </summary>
        private void AddMessageToChat(string message)
        {
            chatTextBox.AppendText(message + Environment.NewLine);
            chatTextBox.SelectionStart = chatTextBox.Text.Length;
            chatTextBox.ScrollToCaret();
        }

        /// <summary>
        /// Обработчик нажатия кнопки Send для отправки текстового сообщения.
        /// </summary>
        private async void sendButton_Click(object sender, EventArgs e)
        {
            string message = inputTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                await _chatClient.SendMessageAsync(message);
                AddMessageToChat("Я: " + message);
                inputTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Введите сообщение перед отправкой!",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиши в поле ввода сообщений.
        /// Если нажата клавиша Enter, вызывается отправка сообщения.
        /// </summary>
        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendButton_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true; // Предотвращает добавление новой строки и системный звук
            }
        }

        /// <summary>
        /// Универсальный обработчик для нажатия кнопок (button1..button8).
        /// Переключает состояние кнопки и отправляет информацию на сервер.
        /// </summary>
        private async void ToggleButtonState(object sender, int buttonNumber)
        {
            if (sender is Button btn)
            {
                bool isOn = (btn.BackColor == Color.Gray);
                btn.BackColor = isOn ? Color.Green : Color.Gray;
                string state = isOn ? "ON" : "OFF";
                string message = $"BUTTON:{buttonNumber}:{state}";
                await _chatClient.SendMessageAsync(message);
                AddMessageToChat($"Я нажал кнопку {buttonNumber} -> {state}");
            }
        }

        // Обработчики кликов для каждой кнопки (предполагается, что они назначены в дизайнере)
        private void button1_Click(object sender, EventArgs e) { ToggleButtonState(sender, 1); }
        private void button2_Click(object sender, EventArgs e) { ToggleButtonState(sender, 2); }
        private void button3_Click(object sender, EventArgs e) { ToggleButtonState(sender, 3); }
        private void button4_Click(object sender, EventArgs e) { ToggleButtonState(sender, 4); }
        private void button5_Click(object sender, EventArgs e) { ToggleButtonState(sender, 5); }
        private void button6_Click(object sender, EventArgs e) { ToggleButtonState(sender, 6); }
        private void button7_Click(object sender, EventArgs e) { ToggleButtonState(sender, 7); }
        private void button8_Click(object sender, EventArgs e) { ToggleButtonState(sender, 8); }

        /// <summary>
        /// Обработчик закрытия формы.
        /// Отписывается от событий и закрывает соединение.
        /// </summary>
        private void ModeratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _chatClient.MessageReceived -= OnMessageReceived;
            _chatClient.Disconnect();
        }
    }
}
