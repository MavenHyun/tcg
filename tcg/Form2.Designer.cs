namespace tcg
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupbox1 = new System.Windows.Forms.GroupBox();
            this.ply_port = new System.Windows.Forms.TextBox();
            this.ply_ip = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.opp_port = new System.Windows.Forms.TextBox();
            this.opp_ip = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listBox_chat = new System.Windows.Forms.ListBox();
            this.btn_send = new System.Windows.Forms.Button();
            this.textBox_msg = new System.Windows.Forms.TextBox();
            this.btn_connect = new System.Windows.Forms.Button();
            this.btn_gameStart = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupbox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupbox1
            // 
            this.groupbox1.Controls.Add(this.ply_port);
            this.groupbox1.Controls.Add(this.ply_ip);
            this.groupbox1.Controls.Add(this.label2);
            this.groupbox1.Controls.Add(this.label1);
            this.groupbox1.Location = new System.Drawing.Point(12, 47);
            this.groupbox1.Name = "groupbox1";
            this.groupbox1.Size = new System.Drawing.Size(242, 99);
            this.groupbox1.TabIndex = 0;
            this.groupbox1.TabStop = false;
            this.groupbox1.Text = "You";
            // 
            // ply_port
            // 
            this.ply_port.Location = new System.Drawing.Point(50, 57);
            this.ply_port.Name = "ply_port";
            this.ply_port.Size = new System.Drawing.Size(179, 21);
            this.ply_port.TabIndex = 3;
            // 
            // ply_ip
            // 
            this.ply_ip.Location = new System.Drawing.Point(50, 30);
            this.ply_ip.Name = "ply_ip";
            this.ply_ip.Size = new System.Drawing.Size(179, 21);
            this.ply_ip.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "PORT";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.opp_port);
            this.groupBox2.Controls.Add(this.opp_ip);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(277, 47);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(242, 99);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Opponent";
            // 
            // opp_port
            // 
            this.opp_port.Location = new System.Drawing.Point(50, 60);
            this.opp_port.Name = "opp_port";
            this.opp_port.Size = new System.Drawing.Size(179, 21);
            this.opp_port.TabIndex = 3;
            // 
            // opp_ip
            // 
            this.opp_ip.Location = new System.Drawing.Point(50, 30);
            this.opp_ip.Name = "opp_ip";
            this.opp_ip.Size = new System.Drawing.Size(179, 21);
            this.opp_ip.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "IP";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "PORT";
            // 
            // listBox_chat
            // 
            this.listBox_chat.FormattingEnabled = true;
            this.listBox_chat.ItemHeight = 12;
            this.listBox_chat.Location = new System.Drawing.Point(12, 208);
            this.listBox_chat.Name = "listBox_chat";
            this.listBox_chat.Size = new System.Drawing.Size(507, 88);
            this.listBox_chat.TabIndex = 2;
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(427, 303);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(92, 23);
            this.btn_send.TabIndex = 3;
            this.btn_send.Text = "Send";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // textBox_msg
            // 
            this.textBox_msg.Location = new System.Drawing.Point(12, 305);
            this.textBox_msg.Name = "textBox_msg";
            this.textBox_msg.Size = new System.Drawing.Size(409, 21);
            this.textBox_msg.TabIndex = 4;
            // 
            // btn_connect
            // 
            this.btn_connect.Location = new System.Drawing.Point(167, 152);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(87, 50);
            this.btn_connect.TabIndex = 5;
            this.btn_connect.Text = "Connect";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Click += new System.EventHandler(this.btn_connect_Click);
            // 
            // btn_gameStart
            // 
            this.btn_gameStart.Location = new System.Drawing.Point(277, 152);
            this.btn_gameStart.Name = "btn_gameStart";
            this.btn_gameStart.Size = new System.Drawing.Size(87, 50);
            this.btn_gameStart.TabIndex = 6;
            this.btn_gameStart.Text = "Game Start";
            this.btn_gameStart.UseVisualStyleBackColor = true;
            this.btn_gameStart.Click += new System.EventHandler(this.btn_gameStart_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(101, 169);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(60, 16);
            this.radioButton1.TabIndex = 7;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "HOST!";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 340);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.btn_gameStart);
            this.Controls.Add(this.btn_connect);
            this.Controls.Add(this.textBox_msg);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.listBox_chat);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupbox1);
            this.Name = "Form2";
            this.Text = "당신은 갓동님 대기실";
            this.groupbox1.ResumeLayout(false);
            this.groupbox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupbox1;
        private System.Windows.Forms.TextBox ply_port;
        private System.Windows.Forms.TextBox ply_ip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox opp_port;
        private System.Windows.Forms.TextBox opp_ip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBox_chat;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.TextBox textBox_msg;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.Button btn_gameStart;
        private System.Windows.Forms.RadioButton radioButton1;
    }
}