using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ClientModeratorApp.Models;



namespace ClientModeratorApp
{
    public partial class LoginForm : Form
    {

            //список пользователей
        private List<User> users = new List<User>
        {
            new User{Username = "model001", Password="1234", Role="Client"},
            new User{Username = "moderator1", Password="5678", Role="Moderator"}

        };


        public LoginForm()
        {
            InitializeComponent();

            // Подключаем обработчики событий для ограничения ввода
            this.usernameTextBox.KeyPress += new KeyPressEventHandler(this.usernameTextBox_KeyPress);
            this.passwordTextBox.KeyPress += new KeyPressEventHandler(this.passwordTextBox_KeyPress);
            this.loginButton.Click += new System.EventHandler(this.loginButton_click);

        }


        private void usernameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем ввод только латинских букв и цифр
            if (!(e.KeyChar >= 'A' && e.KeyChar <= 'Z') &&  // Большие латинские буквы
                !(e.KeyChar >= 'a' && e.KeyChar <= 'z') &&  // Маленькие латинские буквы
                !(e.KeyChar >= '0' && e.KeyChar <= '9') &&  // Цифры
                e.KeyChar != (char)8)                      // Backspace
            {
                e.Handled = true; // Запрещаем ввод
            }
        }

        private void passwordTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем ввод только латинских букв и цифр
            if (!(e.KeyChar >= 'A' && e.KeyChar <= 'Z') &&  // Большие латинские буквы
                !(e.KeyChar >= 'a' && e.KeyChar <= 'z') &&  // Маленькие латинские буквы
                !(e.KeyChar >= '0' && e.KeyChar <= '9') &&  // Цифры
                e.KeyChar != (char)8)                      // Backspace
            {
                e.Handled = true; // Запрещаем ввод
            }
        }




        private void loginButton_click(object sender, EventArgs e)
        {
            string enteredUsername = usernameTextBox.Text.Trim();
            string enteredPassword = passwordTextBox.Text;

            // Поиск пользователя в списке
            var user = users.FirstOrDefault(u => u.Username == enteredUsername && u.Password == enteredPassword);

            if (user != null)
            {
                // Открываем соответствующую форму в зависимости от роли
                if (user.Role == "Client")
                {
                    OpenNextForm(new ClientForm(this)); // Передаём ссылку на LoginForm
                }
                else if (user.Role == "Moderator")
                {
                    OpenNextForm(new ModeratorForm(this));
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void OpenNextForm(Form nextForm)
        {
            this.Hide();
            nextForm.ShowDialog(); // Ожидает закрытия новой формы
            this.Close();          // Закрывает текущую форму после возврата
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label_password_Click(object sender, EventArgs e)
        {

        }

        private void label_head_Click(object sender, EventArgs e)
        {

        }
       



    }
}
