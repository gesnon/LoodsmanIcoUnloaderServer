using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanAPIProjectNew
{
    public class LoodsmanObjectDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Version { get; set; }
        public string State { get; set; }
        public int LoodsmanId { get; set; }
        public string FileName { get; set; }
        public int FileId { get; set; }
        public string PathToFile { get; set; }
        public string Error { get; set; }

        public static LoodsmanObjectDTO Create(DataRow row)
        {
            string Name = row["_PRODUCT"].ToString();
            string Type = row["_TYPE"].ToString();
            decimal Version = decimal.Parse(row["_VERSION"].ToString());
            int LoodsmanId = int.Parse(row["_ID_VERSION"].ToString());
            string State = row["_STATE"].ToString();

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
    }
}
