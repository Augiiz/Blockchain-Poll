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
    public partial class Serveris : Form
    {

        public Blockchain blockChain;
        private string Port;
        private string IP;
        private List<string> IPs;
        private List<string> Ports;
        private bool BlockChainsIsValid = true;
        private object Lock = new object();

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
            label1.Text = label1.Text + " Prisijungęs";
            Thread tpcListenerThread = new Thread(new ThreadStart(tpcListener));
            tpcListenerThread.Start();
        }

        private void tpcListener()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, int.Parse(Port));
            tcpListener.Start();
            this.BeginInvoke((MethodInvoker)delegate
            {
                textBox1.AppendText("Pradėta\r\n");
            });

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                if (client.Connected)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText("Client prisijungė \r\n");
                    });
                }

                Thread tcpHandlerThread = new Thread(new ParameterizedThreadStart(tcpHandler));
                tcpHandlerThread.Start(client);
            }
        }

        private void tcpHandler(object client)
        {
            lock (Lock)
            {
                TcpClient Client = (TcpClient)client;
                NetworkStream stream = Client.GetStream();
                blockChain.origin = "Server";
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("'Client' pasiima pagrindinį 'Blockchain'ą \r\n");
                });

                stream.Write(ObjectToByteArray(blockChain), 0, ObjectToByteArray(blockChain).Length);

                var userChain = new byte[2042];
                int bytes = stream.Read(userChain, 0, userChain.Length);

                var memStream = new MemoryStream();
                memStream.Write(userChain, 0, bytes);

                byte[] BlockChainBytes = new byte[2042];
                BlockChainBytes = memStream.ToArray();

                Blockchain tempBlockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText("Gautas Blockchain \r\n");
                    textBox1.AppendText("Tikrinama ar Blockchain autentiškas \r\n");
                });


                if (CheckIfValid(tempBlockChain))
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText("Blockchain autentiškas \r\n");
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
                        textBox1.AppendText("Blockchain nėra autentiškas. \r\n");
                    });
                }
            }
            if (!BlockChainsIsValid)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    textBox1.AppendText(" Blockchain nėra autentiškas iš serverio atšaukimaa sinchronizacija. \r\n");
                });
            }
            SetGrid(blockChain);
        }

        private void Servers(Blockchain ClientBlockChain)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                textBox1.AppendText("Blockchain tikrinami su kitais serveriais \r\n");
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

                    var response = new byte[2042];
                    int bytes = stream.Read(response, 0, response.Length);

                    var memoryStream = new MemoryStream();
                    memoryStream.Write(response, 0, bytes);

                    byte[] BlockChainBytes = new byte[2042];
                    BlockChainBytes = memoryStream.ToArray();

                    Blockchain OtherServerBlockChain = (Blockchain)ByteArrayToObject(BlockChainBytes);

                    if (CheckIfValid(OtherServerBlockChain))
                    {
                        for (int a = 0; a < OtherServerBlockChain.Chain.Count; a++)
                        {
                            if (OtherServerBlockChain.Chain[a].Hash != ClientBlockChain.Chain[a].Hash)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Nėra autentiškas Blockchain iš: " + serverip.ToString() + " " + serverport.ToString() + "\r\n");
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
                            if (BlockChainsIsValid)
                            {
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Blockchain iš serverio: " + serverip.ToString() + " " + serverport.ToString() + " yra autentiškas\r\n");
                                });
                                ClientBlockChain.origin = "Server";
                                stream.Write(ObjectToByteArray(ClientBlockChain), 0, ObjectToByteArray(ClientBlockChain).Length);
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    textBox1.AppendText("Blockchain apsikeičia su serveriu " + serverip.ToString() + " " + serverport.ToString() + " sėkmingai\r\n");
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
                            textBox1.AppendText("Blockchain iš serverio: " + serverip.ToString() + " " + serverport.ToString() + " nėra autentiškas\r\n");
                        });

                    }
                }
                else
                {
                    MessageBox.Show("Neprisijungęs prie serverio. Bandykite kitą kartą.");
                }
            }
        }

        private void SetGrid(Blockchain blockchain)
        {
            int pasirinkimas1 = 0;
            int pasirinkimas2 = 0;
            int pasirinkimas3 = 0;
            int pasirinkimas4 = 0;
            int pasirinkimas5 = 0;
            int pasirinkimas6 = 0;
            

            foreach (Block blck in blockchain.Chain)
            {
                if (blck.Data == "RADŽI")
                {
                    pasirinkimas1++;
                }
                else if (blck.Data == "MINEDAS")
                {
                    pasirinkimas2++;
                }
                else if (blck.Data == "MONIQUE")
                {
                    pasirinkimas3++;
                }
                if (blck.Data == "DJ ALEX BIT")
                {
                    pasirinkimas4++;
                }
                else if (blck.Data == "SEL")
                {
                    pasirinkimas5++;
                }
                else if (blck.Data == "OSTAPENKO")
                {
                    pasirinkimas6++;
                }
                
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Kandidatai");
            dt.Columns.Add("Rezultatai");
            dt.Rows.Add("RADŽI", pasirinkimas1);
            dt.Rows.Add("MINEDAS", pasirinkimas2);
            dt.Rows.Add("MONIQUE", pasirinkimas3);
            dt.Rows.Add("DJ ALEX BIT", pasirinkimas4);
            dt.Rows.Add("SEL", pasirinkimas5);
            dt.Rows.Add("OSTAPENKO", pasirinkimas6);
            

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
        private void button1_Click(object sender, EventArgs e)
        {
            Vote form = new Vote(IP, Port);
            this.Hide();
            form.ShowDialog();
            this.Show();
            cc = true;
            if(cc==true)button1.Enabled = false; button1.Text = "Jus jau balsavote";
        }

    }
}
