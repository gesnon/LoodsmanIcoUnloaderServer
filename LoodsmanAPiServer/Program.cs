using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LoodsmanAPiServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
           
            LoodsmanService loodsmanService = new LoodsmanService();

            LoodsmanObjectDTO LDO = loodsmanService.GetPath();

            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LoodsmanObjectDTO));
            using (FileStream fs = new FileStream($@"{pathToDocuments}\{LDO.Name}.xml", FileMode.OpenOrCreate))
            {
                xmlSerializer.Serialize(fs, LDO);

                fs.Close();

            }
        }
    }
}
