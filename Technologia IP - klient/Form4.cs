using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Technologia_IP___klient
{
    
public partial class Form4 : Form
    {
        WaveIn wave;
        BufferedWaveProvider bufferedWaveProvider;
        SavingWaveProvider savingWaveProvider;
        WaveOut player;

        public string RoomId;
        private string nick;
        private Form1 form1;
        private string ip;
        List<String> usernames = new List<String>();


        public Form4(Form1 form1,string ip, string nick, string RoomId)
        {
            this.RoomId = RoomId;
            this.nick = nick;
            this.form1 = form1;
            this.ip = ip;
            player = new WaveOut();
            player.Init(savingWaveProvider);
            player.Play();
            InitializeComponent();
            LoadDevices();
        }
        private void Form4_Load(object sender, EventArgs e)
        {
            textBox3.Text = nick;
            textBox2.Text = RoomId;
            textBox1.Text = ip;
            PullState();
            timer.Start();
        }
        public void PullState()
        {
            string sData = CommProtocol.read();
            if (sData == "")
            {
                timer.Stop();
                MessageBox.Show("Connection error");
                Application.Exit();
            }
            string[] logData = CommProtocol.CheckMessage(sData);
            if (logData[0] == "pull")
            {
                for(int i = 2; i < Int32.Parse(logData[1]); i++)
                {
                    usernames.Add(logData[i]);
                }
            }
            else MessageBox.Show("PullState error");
            RefreshDisplay();
        }
        public void RefreshDisplay()
        {
            if (usernames.Count != 0)
            {
                table1.RowCount = usernames.Count;
            }
            for (int i = 0; i < usernames.Count; i++)
            {
                int j = 0;
                table1.Rows[i].Cells[j++].Value = usernames[i];
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            CommProtocol.write("noop");
            PullState();
        }
        private void LoadDevices()
        {
           for (int deviceId = 0; deviceId < WaveIn.DeviceCount; deviceId++)
            {
                var deviceInfo = WaveIn.GetCapabilities(deviceId);
                comboBox1.Items.Add(deviceInfo.ProductName);
            }
            for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
            {
                var deviceInfo = WaveOut.GetCapabilities(deviceId);
                comboBox2.Items.Add(deviceInfo.ProductName);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                wave = new WaveIn();
                wave.DeviceNumber = comboBox1.SelectedIndex;
                wave.DataAvailable += Wave_DataAvailable;
                bufferedWaveProvider = new BufferedWaveProvider(wave.WaveFormat);
                savingWaveProvider = new SavingWaveProvider(bufferedWaveProvider, "temp.wav");
                
                wave.StartRecording();

            }
            else if (!checkBox1.Checked)
            {
                wave.StopRecording();
            }
        }
    private void Wave_RecordingStopped(object sender, RoutedEventArgs e)
    {
            wave.StopRecording();
            player.Stop();
            savingWaveProvider.Dispose();
    }
    private void Wave_DataAvailable(object sender, WaveInEventArgs waveInEventArgs)
    {
            bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
            {
                player.Stop();
                checkBox2.Checked = true;
            }
            else
            {
                player.Play();
                checkBox2.Checked = false;
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                player.Stop();
            }
            else
            {
                player.Play();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Stop();
            CommProtocol.write("lrm");
            this.Close();
            form1.Show();
        }
    }
}
class SavingWaveProvider : IWaveProvider, IDisposable
{
    private readonly IWaveProvider sourceWaveProvider;
    private readonly WaveFileWriter writer;
    private bool isWriterDisposed;

    public SavingWaveProvider(IWaveProvider sourceWaveProvider, string wavFilePath)
    {
        this.sourceWaveProvider = sourceWaveProvider;
        writer = new WaveFileWriter(wavFilePath, sourceWaveProvider.WaveFormat);
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        var read = sourceWaveProvider.Read(buffer, offset, count);
        if (count > 0 && !isWriterDisposed)
        {
            writer.Write(buffer, offset, read);
        }
        if (count == 0)
        {
            Dispose(); // auto-dispose in case users forget
        }
        return read;
    }

    public WaveFormat WaveFormat { get { return sourceWaveProvider.WaveFormat; } }

    public void Dispose()
    {
        if (!isWriterDisposed)
        {
            isWriterDisposed = true;
            writer.Dispose();
        }
    }
}
