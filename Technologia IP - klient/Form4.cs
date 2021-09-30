using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Technologia_IP___klient
{

    public partial class Form4 : Form
    {
        WaveIn wave;
        BufferedWaveProvider bufferedWave;
        SavingWaveProvider savingWaveProvider;
        WaveOut recived;

        public string RoomId;
        private string nick;
        private Form1 form1;
        private string ip;
        private int udpPort;
        UdpClient udpClient;
        IPEndPoint serverEP;
        WaveOut received;
        int counter;
        int framesToPlay = 30 * 32000;  // 30s
        byte[] audio1;
        Stopwatch stopwatch;
        List<String> usernames = new List<String>();

        void SendUDP(byte[] bytes)
        {
            udpClient.Send(bytes, bytes.Length);
        }
        byte[] ReceiveUDP()
        {
            IPEndPoint ep = new IPEndPoint(0, 0);
            return udpClient.Receive(ref ep);
        }
        public Form4(Form1 form1, string ip, string nick, string RoomId, int udpPort)
        {
            this.RoomId = RoomId;
            this.nick = nick;
            this.form1 = form1;
            this.ip = ip;
            this.udpPort = udpPort;
            udpClient = new UdpClient(udpPort);
            serverEP = new IPEndPoint(IPAddress.Parse(ip), 8100);
            udpClient.Connect(serverEP);


            InitializeComponent();
            LoadDevices();
            thread();            //savingWaveProvider = new SavingWaveProvider(bufferedWaveProvider, "temp.wav");
            //player = new WaveOut();
            //player.Init(savingWaveProvider);
            //player.Play(); 
        }
        public void thread()
        {
            System.Collections.Generic.IEnumerable<byte[]> CutTo10ms(byte[] bytes)
            {
                int counter2 = 0;
                while (true)
                {
                    Console.WriteLine("Playback started");
                    for (int i = 0; i < bytes.Length; i += 320)
                    {
                        byte[] index = BitConverter.GetBytes(counter2++);
                        byte[] sample = new byte[324];
                        Array.Copy(index, sample, 4);
                        Array.Copy(bytes, i, sample, 4, 320);
                        yield return sample;
                    }
                }
            }

            counter = 0;
            int playbackCounter = 0;
            stopwatch = new Stopwatch();

            Thread sendingThread = new Thread(unused =>
            {
                while (true)
                {

                    foreach (byte[] elem in CutTo10ms(audio1))
                    {
                        long nextTime = counter * 10;
                        WinApi.TimeBeginPeriod(1);
                        while (stopwatch.ElapsedMilliseconds < nextTime)
                        {
                            Thread.Sleep(1);
                        }
                        WinApi.TimeEndPeriod(1);
                        SendUDP(elem);
                        counter++;
                    }
                }
            });

            Dictionary<int, byte[]> buffer = new Dictionary<int, byte[]>();
            int serverOffset = 0;
            double avgTimeAhead = 0.0;

            bool fresh = true;
            Thread receivingThread = new Thread(unused =>
            {
                while (true)
                {
                    var data = ReceiveUDP();
                    byte[] audio = new byte[320];
                    Array.Copy(data, 4, audio, 0, 320);
                    var index = BitConverter.ToInt32(data, 0);

                    lock (buffer)
                    {
                        if (fresh)
                        {
                            serverOffset = playbackCounter - index;
                            avgTimeAhead = playbackCounter * 10 - stopwatch.ElapsedMilliseconds;
                            fresh = false;
                        }
                        int targetFrame = serverOffset + index;
                        double timeAhead = Math.Max(-50, targetFrame * 10 - stopwatch.ElapsedMilliseconds);
                        avgTimeAhead = 0.99 * avgTimeAhead + 0.01 * timeAhead;
                        //Console.WriteLine("target: " + targetFrame);
                        //Console.WriteLine("offset: " + serverOffset);
                        //Console.WriteLine("we have this much time: " + timeAhead);
                        //Console.WriteLine("avg time: " + avgTimeAhead);

                        if (avgTimeAhead < 10.0)
                        {
                            serverOffset = playbackCounter - index;
                            targetFrame = playbackCounter;
                            avgTimeAhead = playbackCounter * 10 - stopwatch.ElapsedMilliseconds;
                        }
                        if (avgTimeAhead > 30.0)
                        {
                            serverOffset--;
                            targetFrame--;
                            avgTimeAhead -= 10.0;
                        }
                        if (targetFrame >= playbackCounter)
                        {
                            buffer[targetFrame] = audio;
                        }
                    }
                }
            });

            BufferedWaveProvider bufferedWave = new BufferedWaveProvider(new WaveFormat(16000, 16, 1));

            WaveOut received = new WaveOut();
            //Console.WriteLine("Latency: " + received.DesiredLatency);
            //received.DesiredLatency = 100;
            received.Init(bufferedWave);

            Thread playingThread = new Thread(unused =>
            {

                received.Play();
                while (true)
                {
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
                    long nextTime = playbackCounter * 10;
                    WinApi.TimeBeginPeriod(1);
                    while (stopwatch.ElapsedMilliseconds < nextTime)
                    {
                        Thread.Sleep(1);
                    }
                    //Console.WriteLine("woke up late by: " + (stopwatch.ElapsedMilliseconds - nextTime));
                    WinApi.TimeEndPeriod(1);
                    /*WinApi.TimeBeginPeriod(1);
                    Thread.Sleep(1);
                    WinApi.TimeEndPeriod(1);*/
                    lock (buffer)
                    {
                        //Console.WriteLine("playing: " + playbackCounter + " " + buffer.TryGetValue(playbackCounter, out _) + " " + buffer.Count);
                        if (buffer.TryGetValue(playbackCounter, out byte[] sample))
                        {
                            bufferedWave.AddSamples(sample, 0, 320);
                            buffer.Remove(playbackCounter);
                        }
                        playbackCounter++;
                    }
                }
            });

            stopwatch.Start();
            sendingThread.Start();
            receivingThread.Start();
            playingThread.Start();

            playingThread.Join();
        }
        private void Form4_Load(object sender, EventArgs e)
        {
            textBox3.Text = nick;
            textBox2.Text = RoomId;
            textBox1.Text = ip;
            PullState();
            timer1.Start();
        }
        public void PullState()
        {
            string sData = CommProtocol.read();
            if (sData == "")
            {
                timer1.Stop();
                MessageBox.Show("Connection error");
                Application.Exit();
            }
            string[] logData = CommProtocol.CheckMessage(sData);
            if (logData[0] == "pull")
            {
                for (int i = 2; i < Int32.Parse(logData[1]); i++)
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
            CommProtocol.write("pull");
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
                while (true)
                {
                    wave.DataAvailable += new EventHandler<WaveInEventArgs>(Wave_DataAvailable);

                    long nextTime = counter * 10;
                    WinApi.TimeBeginPeriod(1);
                    while (stopwatch.ElapsedMilliseconds < nextTime)
                    {
                        Thread.Sleep(1);
                    }
                    WinApi.TimeEndPeriod(1);
                    byte[] buff = new byte[324];

                }

            }
            else if (!checkBox1.Checked)
            {
                wave.StopRecording();
            }

        }
        private void Wave_RecordingStopped(object sender, RoutedEventArgs e)
        {
            wave.StopRecording();
            recived.Stop();
            savingWaveProvider.Dispose();
        }
        private void Wave_DataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            bufferedWave.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
            {
                received.Stop();
                checkBox2.Checked = true;
            }
            else
            {
                received.Play();
                checkBox2.Checked = false;
            }

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                recived.Stop();
            }
            else
            {
                recived.Play();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            CommProtocol.write("lrm");
            this.Close();
            form1.Show();
        }
    }
}
public static class WinApi
{
    /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

    public static extern uint TimeEndPeriod(uint uMilliseconds);
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
