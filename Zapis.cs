// Trida pro zapis vysledku
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CheckWsnmp
{
    class Zapis
    {
        public string[,] VsParam;

        public Zapis(string[,] vsparam) // konstruktor tridy
        {
            VsParam = vsparam;
        }

        public void ZapisZacatek(string zprava)
        {
            string Zprava = zprava;

            Console.WriteLine("<prtg>");
            Console.WriteLine("<text>{0}</text>", zprava);
        }

        public void ZapisPolozky(string param, string hodnota)
        {
            string Param = param;
            string Hodnota = hodnota;

            Console.WriteLine("<result>");
            Console.WriteLine("<channel>{0}</channel>", Param);
            Console.WriteLine("<value>{0}</value>", Hodnota);
            Console.WriteLine("</result>");
        }

        public void ZapisZaver()
        {
            Console.WriteLine("</prtg>");
        }






    }



}







