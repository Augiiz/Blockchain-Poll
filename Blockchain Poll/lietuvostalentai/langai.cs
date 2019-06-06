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

namespace lietuvostalentai
{
    public partial class langai : Form
    {
        public langai()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            
                List<string> Ports = new List<string>();
                List<string> IPS = new List<string>();
                for (int i = 1; i < 1 + Convert.ToInt32(textBox1.Text); ++i)
                {
                    Ports.Add(i.ToString());
                    IPS.Add("127.0.0.1");
                }
                    
                
                for (int i = 0; i < Convert.ToInt32(textBox1.Text); ++i)
                {
                    string port = Ports[i].ToString();
                    string ip = IPS[i].ToString();
                    new Thread(() => new Serveris(ip, port, IPS, Ports).ShowDialog()).Start();
                }
            this.Close();
        }
    }
}
