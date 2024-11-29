using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientModeratorApp
{
    public partial class ClientForm : Form
    {
        private LoginForm loginForm;

        public ClientForm(LoginForm loginForm)
        {
            InitializeComponent();
            this.loginForm = loginForm; // Сохраняем ссылку на LoginForm, если нужно
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            loginForm.Show(); // Показываем форму логина при закрытии
        }



    }
}
