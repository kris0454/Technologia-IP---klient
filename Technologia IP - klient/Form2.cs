using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Technologia_IP___klient
{
    using static CommProtocol;
    public partial class Form2 : Form
    {
        TcpClient client;
        NetworkStream stream;

        string ip;
        string password;
        string nick;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                MessageBox.Show("Brak adresu serwera");
            }
            else
            {
                ip = textBox3.Text;
                if (textBox1.Text != "")
                {
                    nick = textBox1.Text;
                    if(textBox2.Text != "")
                    {
                        password = textBox2.Text;
                        try
                        {
                            client = new TcpClient();
                            client.Connect(ip, 8080);
                            stream = client.GetStream();
                            CommProtocol.init(stream);
                            if (client.Connected)
                            {
                                sendKey(aes);
                                InitializeComponent();
                            }
                        }
                        catch( Exception ex)
                        {
                            MessageBox.Show("Couldn't connect to the server. Please try again.");
                            this.Close();
                        }
                        write("con " + nick);
                        string msg = read();
                       
                        if(msg == "con ok")
                        {
                            this.Hide();
                            Form1 form1 = new Form1(client, ip, nick);
                            form1.ShowDialog();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Brak podanego nicku");
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
