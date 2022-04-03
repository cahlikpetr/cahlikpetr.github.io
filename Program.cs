 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckWsnmp
{
    class Program
    {
        static void Main(string[] args)
        {

            string zprava = "";
            //string hodnota = "";  string param = "";

            string[,] neco = null;
            string[,] snmp = null;

            bool paramOK = false;
            bool pingOk = false;
            bool uptimeOK = false;

            int SnmpRamOk = 0;
            int SnmpRamPoradOk = 0;
            int SnmpCPUOk = 0;



            if (args.Length >= 2) // kontrola poctu argumentu
            {
                Argumenty TestArg = new Argumenty(args, neco);  // vlozi se console paramerty, ziska jmeno serveru a commnity string
                paramOK = TestArg.UpravParam(); // provede se uprava parametru a kontrola min. delky parametru server a snmp community string

                KontrSnmp TestSnmp = new KontrSnmp(snmp, TestArg.Server, TestArg.SnmpComm); //tady uz pouziji ziskane jsmeno serveru a commnity string
                TestSnmp.UprParSn(); // pripravim parametry tridy KontrSnmp

                if (paramOK)
                {
                    zprava = "Param OK, ";
                    pingOk = TestArg.PingHost(); // test ping

                    if (pingOk)
                    {
                        zprava = zprava + "ping OK, ";
                        uptimeOK = TestSnmp.sysuptime(); // test snmp odpovedi
                        if (uptimeOK) { zprava = zprava + "snmp OK"; } else { zprava = zprava + "snmp Bad"; }

                    }
                    else
                    {
                        zprava = zprava + "ping Bad ";
                    }


                }// dostupnost serveru

                else

                {
                    zprava = "Bad param, ";
                }


                if (pingOk && uptimeOK) //server odpovida
                {

                    //TestArg.KontrSluz(); vynechano, aby jeden senzor neobsahoval velké množství funkcí

                    SnmpRamPoradOk = TestSnmp.PoradRam();
                    SnmpRamOk = TestSnmp.UsedRam();
                    SnmpCPUOk = TestSnmp.CPULoad();

                    Zapis Zapsani = new Zapis(TestArg.VsParam);

                    Zapsani.ZapisZacatek(zprava);

                    for (int i = 0; i < TestSnmp.SnParam.GetLength(1); i++)
                    {
                        Zapsani.ZapisPolozky(TestSnmp.SnParam[0, i], TestSnmp.SnParam[1, i]);
                    }

                    for (int i = 0; i < TestArg.VsParam.GetLength(1); i++) 
                    {
                        Zapsani.ZapisPolozky(TestArg.VsParam[0, i], TestArg.VsParam[1, i]);
                    } 

                    Zapsani.ZapisZaver();
                }
                else  // nespravny tvar argumentu nebo nedostupny server
                {
                    if (paramOK)
                    {
                        Zapis Zapsani = new Zapis(TestArg.VsParam);
                        Zapsani.ZapisZacatek(zprava);
                        for (int i = 0; i < TestSnmp.SnParam.GetLength(1); i++)
                        {
                            Zapsani.ZapisPolozky(TestSnmp.SnParam[0, i], TestSnmp.SnParam[1, i]);
                        }
                        for (int i = 0; i < TestArg.VsParam.GetLength(1); i++) 
                        {
                            Zapsani.ZapisPolozky(TestArg.VsParam[0, i], TestArg.VsParam[1, i]);
                        } 
                        Zapsani.ZapisZaver();
                    }
                    else
                    {
                        zprava = zprava + ", check parameters and target server";
                        Zapis Zapsani = new Zapis(neco);
                        Zapsani.ZapisZacatek(zprava);
                        Zapsani.ZapisZaver();
                    }
                }

            }
            else // chybejici argumenty
            {
                
                    zprava = "Set parameters as \"Server name\" \"Community string\" optional \"Names of the services\" separated by space";
                    Zapis Zapsani = new Zapis(neco);
                    Zapsani.ZapisZacatek(zprava);
                    Zapsani.ZapisZaver();
                
            }


        }

        

    } // class program
}
