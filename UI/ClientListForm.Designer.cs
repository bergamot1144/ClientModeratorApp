namespace ClientModeratorApp
{
    partial class ClientListForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox clientbox;
        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Button buttonRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.clientbox = new System.Windows.Forms.ListBox();
            this.button_connect = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clientbox
            // 
            this.clientbox.FormattingEnabled = true;
            this.clientbox.Location = new System.Drawing.Point(28, 28);
            this.clientbox.Margin = new System.Windows.Forms.Padding(2);
            this.clientbox.Name = "clientbox";
            this.clientbox.Size = new System.Drawing.Size(288, 290);
            this.clientbox.TabIndex = 0;
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(356, 143);
            this.button_connect.Margin = new System.Windows.Forms.Padding(2);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(88, 54);
            this.button_connect.TabIndex = 1;
            this.button_connect.Text = "Connect";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(356, 28);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 2;
            this.buttonTest.Text = "buttonTest";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(388, 295);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 3;
            this.buttonRefresh.Text = "refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // ClientListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 340);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.clientbox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ClientListForm";
            this.Text = "chatVA ClientListForm v0.0.1";
            this.Load += new System.EventHandler(this.ClientListForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
