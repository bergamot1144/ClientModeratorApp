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
    public partial class ModeratorForm : Form
    {
        private LoginForm loginForm;
        public ModeratorForm(LoginForm loginForm)
        {
            InitializeComponent();
            this.loginForm = loginForm;
        }

        private void ModeratorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            loginForm.Show();
        }








    }
}
