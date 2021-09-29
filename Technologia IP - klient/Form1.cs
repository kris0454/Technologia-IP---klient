using Microsoft.VisualBasic;
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
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
        private string ip;
        private string nick;
        public void refresh()
        {
            dataGridView1.Rows.Clear();

            write("ref");
            string[] data = CheckMessage(read());


            int x = 0;
            int numberOfRows = int.Parse(data[x++]);

            if (numberOfRows != 0)
            {
                this.dataGridView1.RowCount = numberOfRows;
            }

            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    this.dataGridView1.Rows[i].Cells[j].Value = data[x++];
                }
            }
        }
        public Form1(TcpClient Client, string ip, string nick)
        {
            this.client = Client;
            stream = Client.GetStream();
            this.ip = ip;
            this.nick = nick;
            textBox1.Text = ip;
            textBox3.Text = nick;
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.ColumnCount = 3;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Rows.Clear();
            refresh();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                string isPrivate = Convert.ToString(selectedRow.Cells[1].Value);
                string cellValue = Convert.ToString(selectedRow.Cells["Id"].Value);
                if (cellValue != "")
                {
                    if (isPrivate == "True")
                    {
                        string content = Interaction.InputBox("Enter Password: ", "Password", "password", 500, 300);
                        char[] vs = content.ToCharArray();
                        for (int i = 0; i < vs.Length; i++)
                        {
                            if (vs[i].ToString() == " ")
                            {
                                MessageBox.Show("Type password without spaces");
                                return;
                            }
                        }
                        write("jrm " + cellValue + " " + nick + " " + content);
                        string msg = CommProtocol.read();
                        if (msg == "ok")
                        {
                            this.Hide();
                            Form4 form4 = new Form4(this,ip, nick, cellValue);
                            form4.ShowDialog();
                        }
                        else if (msg == "error wrong_password")
                        {
                            MessageBox.Show("Wrong password");
                        }
                        else if (msg == "error full")
                        {
                            MessageBox.Show("Selected room is full");
                        }
                    }
                    else
                    {
                        write("jrm " + cellValue + " " + nick + " ");
                        string msg = CommProtocol.read();

                        if (msg == "ok")
                        {
                            this.Hide();
                            Form4 form4 = new Form4(this,ip,nick, cellValue);
                            form4.ShowDialog();
                        }
                        else if (msg == "error full")
                        {
                            MessageBox.Show("Selected room is full");
                        }
                        else MessageBox.Show(msg);
                    }
                }
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            refresh();
        }

        private void EndConnectButton_Click(object sender, EventArgs e)
        {
            write("dsc");
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(client);
            form3.ShowDialog();
            refresh();
        }
    }
}
