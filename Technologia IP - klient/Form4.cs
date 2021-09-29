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
using System.Windows.Forms;

namespace Technologia_IP___klient
{
    public partial class Form4 : Form
    {
        WaveIn wave;
        WaveFileWriter writer;
        string outputFileName;

        public Form4()
        {
            InitializeComponent();
            LoadDevices();
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
                var dialog = new SaveFileDialog();
                dialog.Filter = "Wave files | *.wav";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                outputFileName = dialog.FileName;
                wave = new WaveIn();
                wave.WaveFormat = new WaveFormat(16000, 1);
                wave.DeviceNumber = comboBox1.SelectedIndex;
                wave.DataAvailable += Wave_DataAvailable;
                wave.RecordingStopped += Wave_RecordingStopped;
                writer = new WaveFileWriter(outputFileName, wave.WaveFormat);
                wave.StartRecording();

            }
            else if (!checkBox1.Checked)
            {
                wave.StopRecording();

               // if (outputFileName == null)
                //    return;

                //var processStartInfo = new ProcessStartInfo
                //{
                 //   FileName = Path.GetDirectoryName(outputFileName),
                //    UseShellExecute = true
                //};

               // Process.Start(processStartInfo);
            }
        }
    private void Wave_RecordingStopped(object sender, StoppedEventArgs e)
    {
            writer.Dispose();
    }
    private void Wave_DataAvailable(object sender, WaveInEventArgs e)
    {
        writer.Write(e.Buffer, 0, e.BytesRecorded);
    }
}
}
