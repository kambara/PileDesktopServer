using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace PileDesktopServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            restartServer();
            ////System.Diagnostics.Process.Start("calc");
        }


        delegate void SetTextCallback(string str);
        private void p(String str)
        {
            Debug.WriteLine(str);
            if (this.infoTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(p);
                this.Invoke(d, new object[] { str });
            }
            else
            {
                infoTextBox.Text += str + "\r\n";
                infoTextBox.SelectionStart = infoTextBox.Text.Length;
                infoTextBox.Focus();
                infoTextBox.ScrollToCaret();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = null;
            try
            {
                client = listener.EndAcceptTcpClient(ar);
            }
            catch
            {
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] readBuffer = new byte[client.ReceiveBufferSize]; // default 8192bytes
                stream.Read(readBuffer, 0, readBuffer.Length);
                string msg = System.Text.Encoding.UTF8.GetString(readBuffer).Replace("\0", "");
                
                if (String.IsNullOrEmpty(msg) == false)
                {
                    processStart(msg);
                }
                stream.Close();
            }
            catch
            {
                p("Can't read from stream");
            }
            finally
            {
                client.Close();
            }
            waitAccept();
        }

        private void processStart(String str)
        {
            string[] split = str.Split(new Char [] {'\n'});
            
            try
            {
                if (split.Length > 1)
                {
                    p(">> " + split[0] +" "+split[1]);
                    Process.Start(split[0], split[1]);
                }
                else
                {
                    p(">> " + str);
                    Process.Start(str);
                }
                
            }
            catch (Exception exception)
            {
                p(exception.Message);
            }
        }

        private TcpListener tcpListener = null;
        private void restartServer()
        {
            if (tcpListener != null)
            {
                try
                {
                    //tcpListener.EndAcceptTcpClient(null);
                    p("Server Stop\r\n---");
                    tcpListener.Stop();
                }
                catch (SocketException exception)
                {
                    p(exception.Message);
                }
            }

            int port = decimal.ToInt32(this.portNumberBox.Value);
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                
                p("Server Start on port " + port.ToString());
                waitAccept();
            }
            catch (SocketException exception)
            {
                p("Socket Error: " + exception.Message);
            }
        }

        private void waitAccept()
        {
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), tcpListener);
            //p("Waiting for a connection..");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            restartServer();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            this.Activate();
            //this.ShowInTaskbar = true;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            this.ShowInTaskbar = this.Visible;
        }
    }
}