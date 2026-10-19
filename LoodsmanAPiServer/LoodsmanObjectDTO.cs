using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanAPiServer
{
    public class LoodsmanObjectDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public string State { get; set; }
        public int LoodsmanId { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
        public string PathToFile { get; set; }
        public string Error { get; set; }

        public static LoodsmanObjectDTO Create(DataRow row)
        {

            string Name = row["_PRODUCT"].ToString();
            log(Name);
            string Type = row["_TYPE"].ToString();
            log(Type);
            string Version = row["_VERSION"].ToString();
            log(Version.ToString());
            int LoodsmanId = int.Parse(row["_ID_VERSION"].ToString());
            log(LoodsmanId.ToString());
            string State = row["_STATE"].ToString();
            log(State.ToString());


            return new LoodsmanObjectDTO { Name = Name, Type = Type, Version = Version, LoodsmanId = LoodsmanId, State = State, PathToFile = "", Error = "" };
        }
        public static List<LoodsmanObjectDTO> CreateList(DataTable dt)
        {
            List<LoodsmanObjectDTO> result = new List<LoodsmanObjectDTO>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(Create(row));
            }
            return result;
        }
        public static void log(string message)
        {
        }
    }
}
