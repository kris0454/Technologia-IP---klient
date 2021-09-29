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
            if (textBox3.Text != "")
            {
                MessageBox.Show("Brak adresu serwera");
            }
            else
            {
                ip = textBox3.Text;

            }

        }
    }
}
