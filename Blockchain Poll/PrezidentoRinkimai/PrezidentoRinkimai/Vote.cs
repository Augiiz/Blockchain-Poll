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

namespace PrezidentoRinkimai
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

                //Reading blockchain from server
                var response = new byte[2042];
                //Read bytes to userChain and return readed bytes number.
                int bytes = stream.Read(response, 0, response.Length);

                var memoryStream = new MemoryStream();
                //Write to memory stream from 0 to readed number bytes.
                memoryStream.Write(response, 0, bytes);

                byte[] BlockChainBytes = new byte[2042];
                //Convert to byte array.
                BlockChainBytes = memoryStream.ToArray();

                blockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);
                blockChain.origin = "Client";

                //Write data to client network stream
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
                    if (radioButton7.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton7.Text));
                    }
                    if (radioButton8.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton8.Text));
                    }
                    if (radioButton9.Checked)
                    {
                        blockChain.AddBlock(new Block(null, radioButton9.Text));
                    }
                    // Sending blockchain to server.
                    blockChain.origin = "Client";
                    stream.Write(ObjectToByteArray(blockChain), 0, ObjectToByteArray(blockChain).Length);

                    //MessageBox.Show("Sekmingai balsuota");

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
                MessageBox.Show("Not connected to server. Try next time.");
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
