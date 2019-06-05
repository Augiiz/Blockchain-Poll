using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace PrezidentoRinkimai
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            List<string> IPs = new List<string>();
            List<string> Ports = new List<string>();
            IPs.Add("127.0.0.1");
            IPs.Add("127.0.0.1");
            IPs.Add("127.0.0.1");
            Ports.Add("8080");
            Ports.Add("8081");
            Ports.Add("8082");
            for (int i = 0; i < Ports.Count; i++)
            {
                string port = Ports[i].ToString();
                string ip = IPs[i].ToString();
                new Thread(() => new Serveris(ip, port, IPs, Ports).ShowDialog()).Start();
            }
        }
    }
}
