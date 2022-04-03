using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Lextm.SharpSnmpLib;
//using Lextm.SharpSnmpLib.Security;
using SnmpSharpNet;

namespace CheckWsnmp
{
    class KontrSnmp
    {
        public String[,] SnParam;   // tak je promenna dostupna i mimo tridu 
        String CisloRAM;
        String TServer;
        String SnmpCom;
              
        public KontrSnmp(string[,] snparam,string serv, string snmpcom) // konstruktor tridy
        {

            SnParam = snparam;
            TServer = serv;
            SnmpCom = snmpcom;
        }

        public void UprParSn() // prevod argumentu na parametry
        {
            SnParam = new string[2, 5]; // Dvourozmerne pole pro snmp hodnoty

            SnParam[0, 0] = "RAM Free %"; 
            SnParam[1, 0] = "51"; // hodnota, když je server nedostupny
            SnParam[0, 1] = "RAM Size MB"; 
            SnParam[1, 1] = "0";
            SnParam[0, 2] = "CPU Load %"; 
            SnParam[1, 2] = "51"; // hodnota, když je server nedostupny
            SnParam[0, 3] = "CPU Count"; 
            SnParam[1, 3] = "0";
            SnParam[0, 4] = "Uptime Days"; 
            SnParam[1, 4] = "0";



        } // end UpravParam

        public int CPULoad() // snmp walk vsechny CPU
        {
            int PracCPUL = 0;
            int PocetCPU = 0;
            double LoadCPU = 0;

            OctetString community = new OctetString(SnmpCom); // SNMP community name - BUDE parametr
            AgentParameters param = new AgentParameters(community); // Define agent parameters class

            param.Version = SnmpVersion.Ver2; // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)

            IpAddress agent = new IpAddress(TServer); // Construct the agent address object BUDE ziskano z DNS jmena
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);  // Construct target    

            Oid rootOid = new Oid("1.3.6.1.2.1.25.3.3.1.2");  //"Name/OID: hrProcessorLoad" Define Oid that is the root of the MIB  tree you wish to retrieve
            Oid lastOid = (Oid)rootOid.Clone();               //This Oid represents last Oid returned by the SNMP agent

            Pdu pdu = new Pdu(PduType.GetBulk); // Pdu class used for all requests

            pdu.NonRepeaters = 0;   // In this example, set NonRepeaters value to 0
            pdu.MaxRepetitions = 10; // MaxRepetitions tells the agent how many Oid/Value pairs to return in the response.
                                     // When Pdu class is first constructed, RequestId is set to 0 and during encoding id will be set to the random value
                                     // for subsequent requests, id will be set to a value that needs to be incremented to have unique request ids for each packet
            if (pdu.RequestId != 0)
            {
                pdu.RequestId += 1;
            }
             pdu.VbList.Clear(); // Clear Oids from the Pdu class.
             pdu.VbList.Add(lastOid); // Initialize request PDU with the last retrieved Oid
            List<int> partCPULoad = new List<int>();
            // Make SNMP request
            try
            {
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                // You should catch exceptions in the Request if using in real application                                    
                if (result != null)  // If result is null then agent didn't reply or we couldn't parse the reply.
                {
                    // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)  // agent reported an error with the request  result.Pdu.ErrorStatus,  result.Pdu.ErrorIndex);
                    {   
                        lastOid = null;
                        target.Close();
                        return 30;
                    }
                    else
                    {

                        foreach (Vb v in result.Pdu.VbList) // Walk through returned variable bindings
                        {
                            if (rootOid.IsRootOf(v.Oid)) // Check that retrieved Oid is "child" of the root OID
                            {
                                //PracSnmp = v.Value.ToString(); // hodnota string napr. F:, Virtual Memory" "Physical Memory
                                PracCPUL = int.Parse(v.Value.ToString());
                                partCPULoad.Add(PracCPUL);

                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)    lastOid = null;
                                else lastOid = v.Oid;
                            }
                            else
                            {
                                lastOid = null; // we have reached the end of the requested MIB tree. Set lastOid to null and exit loop
                            }
                        }

                        LoadCPU = Convert.ToInt32(partCPULoad.Average());
                        PocetCPU = partCPULoad.Count();
                        SnParam[1, 2] = LoadCPU.ToString();
                        SnParam[1, 3] = PocetCPU.ToString();

                        target.Close();
                        return 100;
                    }

                }
                else
                {
                    //Console.WriteLine("No response received from SNMP agent.");
                    target.Close();
                    return 20;
                }

            }
            catch (Exception)
            {
                return 10;
            }

        } // end CPU Load

        public bool  sysuptime() //snmp get 
        {
            int  Vysled;
            bool Zpet;
            TimeSpan uptime;
            IpAddress agent = new IpAddress(TServer);
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);

            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0");
            AgentParameters param = new AgentParameters(SnmpVersion.Ver2, new OctetString(SnmpCom));
        try
            {
                SnmpV2Packet packet = (SnmpV2Packet)target.Request(pdu, param);
                AsnType asnType = packet.Pdu.VbList["1.3.6.1.2.1.1.3.0"].Value;
                uptime = (TimeSpan)(asnType as TimeTicks);
                Vysled = (int)uptime.TotalDays;
                Zpet = true;
                SnParam[1, 4] = Vysled.ToString();
            }
        catch (Exception)
        {
            Zpet = false;
        }         
            return Zpet;
        } // end sysuptime
                      

        public int UsedRam() // snmp get 
        {
            Int32 NavrKod, Celkvyst, ProcPouzVyst;
            float Celk, Unit, Veli,Pouz, ProcPouz;
            string AlUnit,StorSz,StorUs;

            OctetString community = new OctetString(SnmpCom); // SNMP community name        
            AgentParameters param = new AgentParameters(community); // Define agent parameters class
            param.Version = SnmpVersion.Ver2; // Set SNMP version to 1 (or 2)
            IpAddress agent = new IpAddress(TServer); // Construct the agent address object IpAddress class is easy to use here because  it will try to resolve constructor parameter if it doesn't parse to an IP address
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);    // Construct target  
            Pdu pdu = new Pdu(PduType.Get);   // Pdu class used for all requests
            AlUnit = "1.3.6.1.2.1.25.2.3.1.4." + CisloRAM;
            StorSz = "1.3.6.1.2.1.25.2.3.1.5." + CisloRAM;
            StorUs = "1.3.6.1.2.1.25.2.3.1.6." + CisloRAM;
            
            pdu.VbList.Add(AlUnit); // sysObjectID AllocationUnits The size, in bytes
            pdu.VbList.Add(StorSz); //sysObjectID Storage Size
            pdu.VbList.Add(StorUs); //sysObjectID Strorage Used
            
            SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);  // Make SNMP request
            try
            {
                if (result != null) // kdyz je vracen nenulovy objekt If result is null then agent didn't reply or we couldn't parse the reply.
                {
                    // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0) // kdyz je chybovy kod ruzny od 0
                    {
                        
                        NavrKod = 30; // chyby v odpovedi SNMP, agent reported an error with the request result.Pdu.ErrorStatus,result.Pdu.ErrorIndex
                    }
                    else
                    {
                        // Reply variables are returned in the same order as they were added to the VbList
                        Unit = float.Parse(result.Pdu.VbList[0].Value.ToString());
                        Veli = float.Parse(result.Pdu.VbList[1].Value.ToString());
                        Pouz = float.Parse(result.Pdu.VbList[2].Value.ToString());
                        

                        ProcPouz = 100 - (Pouz * 100 / Veli); // - Funkcni
                        ProcPouzVyst = Convert.ToInt32(ProcPouz);

                        Celk = (Veli * Unit) / 1000000;
                        Celkvyst = Convert.ToInt32(Celk);
                       
                        SnParam[1, 0] = ProcPouzVyst.ToString();    // vyuziti RAV v % 
                        SnParam[1, 1] = Celkvyst.ToString();        // celkova velikost RAM
                        

                        NavrKod = 100; // korektni snmp odpoved
                    }

                }
                else
                {
                    //Console.WriteLine("No response received from SNMP agent.");
                    NavrKod = 20; //zadna odpoved SNMP 
                }

                target.Close();
                return NavrKod;
            }
            catch (Exception)
            {
                NavrKod = 10; //chyba SNMP   
                return NavrKod;
            }

        } // end Used RAM

       
        public int  PoradRam() // smtp walk ziska poradi "Physical memory"
        {
            String PoradiRAM;
            String PracSnmp = "";
            OctetString community = new OctetString(SnmpCom); // SNMP community name - BUDE parametr
            AgentParameters param = new AgentParameters(community); // Define agent parameters class

            param.Version = SnmpVersion.Ver2; // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)

            IpAddress agent = new IpAddress(TServer); // Construct the agent address object BUDE ziskano z DNS jmena
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);  // Construct target    

            Oid rootOid = new Oid("1.3.6.1.2.1.25.2.3.1.3");  //"sysObjectID hrStorageDescr" Define Oid that is the root of the MIB  tree you wish to retrieve
            Oid lastOid = (Oid)rootOid.Clone();               //This Oid represents last Oid returned by the SNMP agent
            
            Pdu pdu = new Pdu(PduType.GetBulk); // Pdu class used for all requests

            pdu.NonRepeaters = 0;   // In this example, set NonRepeaters value to 0
            pdu.MaxRepetitions = 10; // MaxRepetitions tells the agent how many Oid/Value pairs to return in the response.
                                     // When Pdu class is first constructed, RequestId is set to 0 and during encoding id will be set to the random value
                                     // for subsequent requests, id will be set to a value that needs to be incremented to have unique request ids for each packet
            if (pdu.RequestId != 0)
            {
                pdu.RequestId += 1;
            }
            // Clear Oids from the Pdu class.
            pdu.VbList.Clear();
            // Initialize request PDU with the last retrieved Oid
            pdu.VbList.Add(lastOid);
            // Make SNMP request
            try
            {
                SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
                // You should catch exceptions in the Request if using in real application                                    
                if (result != null)  // If result is null then agent didn't reply or we couldn't parse the reply.
                {
                    // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request ,  Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                        //    result.Pdu.ErrorStatus,  result.Pdu.ErrorIndex);
                        lastOid = null;
                        target.Close();
                        return 30;
                    }
                    else
                    {
                        
                        foreach (Vb v in result.Pdu.VbList) // Walk through returned variable bindings
                        {                          
                            if (rootOid.IsRootOf(v.Oid)) // Check that retrieved Oid is "child" of the root OID
                            {
                                PoradiRAM = v.Oid.ToString();  // hodnota tvaru 1.3.6.1.2.1.25.2.3.1.3.6
                                PracSnmp = v.Value.ToString(); // hodnota string napr. F:, Virtual Memory" "Physical Memory
                                PoradiRAM = PoradiRAM.Substring(PoradiRAM.Length - 1, 1); // zde ziskam poradi hodnoty 
                                if (PracSnmp == "Physical Memory")
                                { CisloRAM = PoradiRAM; } // kdyz je to "Physical Memory" ulozim hodnotu

                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                    lastOid = null;
                                else
                                    lastOid = v.Oid;
                            }
                            else
                            {
                                lastOid = null; // we have reached the end of the requested MIB tree. Set lastOid to null and exit loop
                            }
                        }

                        target.Close();
                        return 100;
                    }

                }
                else
                {
                    //Console.WriteLine("No response received from SNMP agent.");
                    target.Close();
                    return 20;
                }

            }
            catch (Exception)
            {
                return 10;
            }

           }

        } // end poradi RAM
                    

  }


