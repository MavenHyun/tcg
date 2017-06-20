using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace tcg
{
    public partial class Form2 : Form
    {
        public static Form2 setup_form;
        public Socket sck;
        public EndPoint epLocal, epRemote;
        public bool server = false;

        public Form2()
        {
            setup_form = this;
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listBox_chat.Items.Add("Only person can check HOST!");
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                if (size > 0)
                {
                    byte[] receivedData = new byte[1464];

                    receivedData = (byte[])aResult.AsyncState;

                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);

                    listBox_chat.Items.Add("상대: " + receivedMessage);
                }

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(ply_ip.Text), Convert.ToInt32(ply_port.Text));
                sck.Bind(epLocal);

                epRemote = new IPEndPoint(IPAddress.Parse(opp_ip.Text), Convert.ToInt32(opp_port.Text));
                sck.Connect(epRemote);

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                btn_connect.Text = "Connected!";
                btn_connect.Enabled = false;
                btn_send.Enabled = true;
                textBox_msg.Focus();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }

        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textBox_msg.Text);

                sck.Send(msg);
                listBox_chat.Items.Add("당신: " + textBox_msg.Text);
                textBox_msg.Clear();
            }

            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            server = true;
        }

        private void btn_gameStart_Click(object sender, EventArgs e)
        {
            Form m = new Form1();
            m.Show();
        }


    }
}
