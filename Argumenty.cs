// Trida pro kontrolu vsupnich parametru
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Diagnostics;
using System.Management;
using System.Management.Instrumentation;
using System.Net.NetworkInformation;



namespace CheckWsnmp
{
    class Argumenty
    {
        private string[] Args;      // parametry zadane na prikazova radce
        private int pocetRadku;
        public string Server;       // tak je promenna dostupna i mimo tridu
        public string SnmpComm;
        public string[,] VsParam;   // tak je promenna dostupna i mimo tridu
        bool[] result = new bool[5];
            

    public Argumenty(string[] args, string[,] vsparam) // konstruktor tridy
        {
            Args = args;
            VsParam = vsparam;
        }

        public bool UpravParam() // prevod argumentu na parametry
        {
            pocetRadku = Args.Count()-1; // pocet Args.Count()-1 je pro arg. tykajici se service
            VsParam = new string[2, pocetRadku]; // Dvourozmerne pole pro vstupni parametry
            if (Args[0].Length > 2 && Args[1].Length > 2) // agrument server a community string
            {
                Server = Args[0];
                SnmpComm = Args[1];
                VsParam[0, 0] = "Ping"; // pole pro ping
                VsParam[1, 0] = "0";
                /*for (int i = 1; i < pocetRadku; i = i + 1) // pole pro services
                {
                    VsParam[0, i] = Args[i + 1];
                    VsParam[1, i] = "0";
                }*/
                return true; 
            }
            else
            {
                return false;
            }

        }
        // end UpravParam

        // Ping
        public bool PingHost()
        {
            bool pingable = false;
            bool sumresult = false;
            Ping pinger = null;
            for (int i = 0; i < 5; i++)
            { result[i] = false; }
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    pinger = new Ping();
                    PingReply reply = pinger.Send(Server); // tato podoba ceka 5 sekund           
                    pingable = reply.Status == IPStatus.Success;

                    if (pingable)
                    {
                        result[i] = true;
                    }
                }
                catch (PingException)
                {
                    // Discard PingExceptions and return false;
                }
                finally
                {
                    if (pinger != null) { pinger.Dispose(); }
                }
            }

            if (result.Contains(true))
            {
                VsParam[1, 0] = "100";
                sumresult = true;
            }

            return sumresult;
        }

        public bool KontrSluz()
        {
            bool ParamOK = true;
            for (int i = 1; i < VsParam.GetLength(1); i++)
            {
                try
                {
                    ServiceController sc = new ServiceController(VsParam[0, i], Server);
                    if (sc.Status.ToString() == "Running")
                    {
                        VsParam[1, i] = "100";
                    }
                }
                catch
                {
                    ParamOK = false;
                }
            }
            return ParamOK;
        }
        // end KontrSluz






    }
}
