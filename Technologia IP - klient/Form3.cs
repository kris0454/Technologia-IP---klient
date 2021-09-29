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
    public partial class Form3 : Form
    {
        private TcpClient Client;
        NetworkStream stream;

        public Form3(TcpClient client)
        {
            this.Client = client;
            stream = client.GetStream();
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                char[] vs = textBox3.Text.ToCharArray();
                for (int i = 0; i < vs.Length; i++)
                {
                    if (vs[i].ToString() == " ")
                    {
                        MessageBox.Show("Did you remove all spaces in your password? If not, please delete them.");
                        textBox3.Clear();
                        return;
                    }
                }
                write("crm " + checkBox3.Checked.ToString() + " " + textBox3.Text);
            }
            else write("crm " + checkBox3.Checked.ToString() + " ");


            MessageBox.Show("Pokój stworzony z numerem: " + read());
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
