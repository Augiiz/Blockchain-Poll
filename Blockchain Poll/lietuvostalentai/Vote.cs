using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GeriausiasDainininkas
{
    public partial class Vote : Form
    {
        public Blockchain blockChain = new Blockchain();
        private string IP;
        private string Port;

        public Vote(string IP, string Port)
        {
            this.IP = IP;
            this.Port = Port;
            InitializeComponent();
        }

        private void Client()
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(IP), int.Parse(Port));
            if (client.Connected)
            {
                NetworkStream stream = client.GetStream();

                var response = new byte[2042];
                int bytes = stream.Read(response, 0, response.Length);

                var memoryStream = new MemoryStream();
                memoryStream.Write(response, 0, bytes);

                byte[] BlockChainBytes = new byte[2042];
                BlockChainBytes = memoryStream.ToArray();

                blockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);
                blockChain.origin = "Client";

                try
                {
                    if (radioButton1.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton1.Text));
                    }
                    if (radioButton2.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton2.Text));
                    }
                    if (radioButton3.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton3.Text));
                    }
                    if (radioButton4.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton4.Text));
                    }
                    if (radioButton5.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton5.Text));
                    }
                    if (radioButton6.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton6.Text));
                    }
                    
                    blockChain.origin = "Client";
                    stream.Write(ObjectToByteArray(blockChain), 0, ObjectToByteArray(blockChain).Length);

                    this.BeginInvoke((MethodInvoker)delegate {
                        this.Close();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Neprisijunges prie serverio. Bandykit kitą kartą.");
            }
        }



        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private void kanditatas3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread ClientasThread = new Thread(new ThreadStart(Client));
            ClientasThread.Start();
        }
    }
}
