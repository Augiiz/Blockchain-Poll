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
    public partial class Serveris : Form
    {

        public Blockchain blockChain;
        private string Port;
        private string IP;
        private List<string> IPs;
        private List<string> Ports;
        private bool BlockChainsIsValid = true;

        //For locking threads.
        private object Locks = new object();

        public Serveris(string IP, string Port, List<string> IPs, List<string> Ports)
        {
            InitializeComponent();
            this.IP = IP;
            this.Port = Port;
            this.label1.Text = Port;
            this.IPs = IPs;
            this.Ports = Ports;
            blockChain = new Blockchain();


        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            label1.Text = label1.Text + " is connected";
            Thread tpcListenerThread = new Thread(new ThreadStart(tpcListener));
            tpcListenerThread.Start();
        }

        private void tpcListener()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, int.Parse(Port));
            tcpListener.Start();
            this.BeginInvoke((MethodInvoker)delegate
            {
                textBox1.AppendText("Started \r\n");
            });

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                if (client.Connected)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText("User connected \r\n");
                    });
                }

                Thread tcpHandlerThread = new Thread(new ParameterizedThreadStart(tcpHandler));
                tcpHandlerThread.Start(client);
            }
        }

        private void tcpHandler(object client)
        {
            lock (Locks)
            {
                TcpClient Client = (TcpClient)client;
                NetworkStream stream = Client.GetStream();
                blockChain.origin = "Server";
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("Client taking main blockchain \r\n");
                });

                //Sending blockchain to client
                stream.Write(ObjectToByteArray(blockChain), 0, ObjectToByteArray(blockChain).Length);

                //Reading blockchain from client
                var userChain = new byte[2042];
                //Read bytes to userChain and return readed bytes number.
                int bytes = stream.Read(userChain, 0, userChain.Length);

                //Write to memory stream from 0 to readed number bytes.
                var memStream = new MemoryStream();
                memStream.Write(userChain, 0, bytes);

                byte[] BlockChainBytes = new byte[2042];
                //Convert to byte array.
                BlockChainBytes = memStream.ToArray();

                Blockchain tempBlockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("Got blockchain \r\n");
                    textBox1.AppendText("Checking if blockchain is valid \r\n");
                });


                if (CheckIfValid(tempBlockChain))
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText("Blockchain is valid \r\n");
                    });
                    blockChain = tempBlockChain;
                    if (tempBlockChain.origin.ToString() == "Client")
                    {
                        Servers(tempBlockChain);
                    }
                }
                else
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText("Blockchain is not valid \r\n");
                    });
                }
            }
            if (!BlockChainsIsValid)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("Not valid block chain from server synchronization cancel \r\n");
                });
            }
            SetGrid(blockChain);
        }

        private void Servers(Blockchain ClientBlockChain)
        {
            //Serveris atsiuncia tuscia blockchain
            this.BeginInvoke((MethodInvoker)delegate
            {
                textBox1.AppendText("Checking chain blocks with other servers \r\n");
            });
            for (int i = 0; i < Ports.Count; i++)
            {
                string serverip = IPs[i];
                string serverport = Ports[i];
                if (serverip == IP && serverport == Port)
                {
                    continue;
                }

                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(IPs[i]), int.Parse(Ports[i]));
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

                    Blockchain OtherServerBlockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);

                    if (CheckIfValid(OtherServerBlockChain))
                    {
                        //Chech blocks from other servers
                        for (int a = 0; a < OtherServerBlockChain.Chain.Count; a++)
                        {
                            if (OtherServerBlockChain.Chain[a].Hash != ClientBlockChain.Chain[a].Hash)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Not valid block chain from: " + serverip.ToString() + " " + serverport.ToString() + "\r\n");
                                });
                                BlockChainsIsValid = false;
                            }
                            if (!BlockChainsIsValid)
                            {
                                continue;
                            }
                        }
                        try
                        {
                            // Sending blockchain to server.
                            if (BlockChainsIsValid)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Blockchain from server: " + serverip.ToString() + " " + serverport.ToString() + " is valid\r\n");
                                });
                                ClientBlockChain.origin = "Server";
                                stream.Write(ObjectToByteArray(ClientBlockChain), 0, ObjectToByteArray(ClientBlockChain).Length);
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Blockchain exchange with server: " + serverip.ToString() + " " + serverport.ToString() + " successfull\r\n");
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            textBox1.AppendText("Blockchain from server: " + serverip.ToString() + " " + serverport.ToString() + " not valid\r\n");
                        });

                    }
                }
                else
                {
                    MessageBox.Show("Not connected to server. Try next time.");
                }
            }
        }

        private void SetGrid(Blockchain blockchain)
        {
            int kandidatas1Rez = 0;
            int kandidatas2Rez = 0;
            int kandidatas3Rez = 0;
            int kandidatas4Rez = 0;
            int kandidatas5Rez = 0;
            int kandidatas6Rez = 0;
            int kandidatas7Rez = 0;
            int kandidatas8Rez = 0;
            int kandidatas9Rez = 0;


            foreach (Block blck in blockchain.Chain)
            {
                if (blck.Data == "I. Šimonytė")
                {
                    kandidatas1Rez++;
                }
                else if (blck.Data == "G. Nausėda")
                {
                    kandidatas2Rez++;
                }
                else if (blck.Data == "S. Skvernelis")
                {
                    kandidatas3Rez++;
                }
                if (blck.Data == "A. Juozaitis")
                {
                    kandidatas4Rez++;
                }
                else if (blck.Data == "V. Andriukaitis")
                {
                    kandidatas5Rez++;
                }
                else if (blck.Data == "V. Tomaševski")
                {
                    kandidatas6Rez++;
                }
                if (blck.Data == "N. Puteikis")
                {
                    kandidatas7Rez++;
                }
                else if (blck.Data == "V. Mazuronis")
                {
                    kandidatas8Rez++;
                }
                else if (blck.Data == "M. Puidokas")
                {
                    kandidatas9Rez++;
                }
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Kandidatai");
            dt.Columns.Add("Rezultatai");
            dt.Rows.Add("I. Šimonytė", kandidatas1Rez);
            dt.Rows.Add("G. Nausėda", kandidatas2Rez);
            dt.Rows.Add("S. Skvernelis", kandidatas3Rez);
            dt.Rows.Add("A. Juozaitis", kandidatas4Rez);
            dt.Rows.Add("V. Andriukaitis", kandidatas5Rez);
            dt.Rows.Add("V. Tomaševski", kandidatas6Rez);
            dt.Rows.Add("N. Puteikis", kandidatas7Rez);
            dt.Rows.Add("V. Mazuronis", kandidatas8Rez);
            dt.Rows.Add("M. Puidokas", kandidatas9Rez);

            this.BeginInvoke((MethodInvoker)delegate
            {
                dataGridView1.DataSource = dt;

                dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Descending);
            });
        }


        public bool CheckIfValid(Blockchain blockChain)
        {
            for (int i = 1; i < blockChain.Chain.Count; i++)
            {
                Block currentBlock = blockChain.Chain[i];
                Block previousBlock = blockChain.Chain[i - 1];

                if (currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
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

        bool cc = false;
        private void button69_Click(object sender, EventArgs e)
        {
            Vote form = new Vote(IP, Port);
            this.Hide();
            form.ShowDialog();
            this.Show();
            cc = true;
            if(cc==true)button69.Enabled = false; button69.Text = "Jus jau balsavote";
        }

    }
}
