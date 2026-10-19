using Ascon.Plm.DataPacket;
using Ascon.Plm.ServerApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LoodsmanAPIProject
{
    public class LoodsmanService
    {
        string PathToLoodsmanDownloadFiles = "";

        public LoodsmanObjectDTO GetPath()
        {
           
            string Name = "15ГК.380.999";
            ConnectionFactory _connectionFactory = new ConnectionFactory();
            using (var api = _connectionFactory.CreateConnection("waru16397", 8076))
            {
                object apiErrorCode, apiErrorMessage;
                api.MainSystem.ConnectToDB("ElSh", out apiErrorCode, out apiErrorMessage);//Подключаемся к БД, логин и пароль можно оставить пустыми для Win-входа

                if (string.IsNullOrEmpty(apiErrorMessage.ToString()))
                {
                    return new LoodsmanObjectDTO { Error="нет свободных лицензий"};
                }
                else
                {
                    string CheckDBName = api.MainSystem.CurrentBase.ToString();

                    object ErrorObjectCode;
                    object ErrorObjectMessage;

                    byte[] ObjectsFromLoodsman = (byte[])api.MainSystem.GetVersionList("Деталь", "15ГК.380.999", out ErrorObjectCode, out ErrorObjectMessage);
                    DataTable ObjectsFromLoodsmanDT = ConvertToDataTable(ObjectsFromLoodsman);

                    List<LoodsmanObjectDTO> loodsmanObjectDTOs = LoodsmanObjectDTO.CreateList(ObjectsFromLoodsmanDT);
                    List<LoodsmanObjectDTO> onlyApprovedObjects = loodsmanObjectDTOs.Where(_ => _.State == "Утвержден").ToList();
                    LoodsmanObjectDTO currentObject = onlyApprovedObjects.OrderByDescending(_ => _.Version).First();

                    if (currentObject == null)
                    {
                        return new LoodsmanObjectDTO { Error = "Не найдено объекта" };
                        
                    }
                    else
                    {
                        byte[] documents = (byte[])api.MainSystem.GetLinkedObjectsAndFiles(currentObject.Type, currentObject.Name, currentObject.Version.ToString(), "Документы", false, false, out ErrorObjectCode, out ErrorObjectCode);

                        LoodsmanObjectDTO mainDetail = new LoodsmanObjectDTO();
                        List<LoodsmanObjectDTO> allDocuments = new List<LoodsmanObjectDTO>();
                        DataTable documentsTable = ConvertToDataTable(documents);

                        for (int i = 0; i < documentsTable.Rows.Count; i++)
                        {
                            string IsDocument = documentsTable.Rows[i]["_DOCUMENT"].ToString();

                            try
                            {
                                if (IsDocument != "1")
                                {
                                    string FileName = documentsTable.Rows[i]["_NAME"].ToString();
                                    allDocuments.Add(new LoodsmanObjectDTO { Type = "Файл", FileName = FileName });
                                    continue;
                                }

                            }
                            catch (Exception ex)
                            {
                            }
                            try
                            {
                                string Type1 = documentsTable.Rows[i]["_TYPE"].ToString();
                                string Name1 = documentsTable.Rows[i]["_PRODUCT"].ToString();
                                decimal Version1 = decimal.Parse(documentsTable.Rows[i]["_VERSION"].ToString());
                                int Id1 = (int)documentsTable.Rows[i]["_ID_VERSION"];
                                allDocuments.Add(new LoodsmanObjectDTO { Name = Name1, Type = Type1, LoodsmanId = Id1, Version = Version1 });
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        for (int i = 0; i < allDocuments.Count; i++)
                        {

                            if (allDocuments[i].Type == "3D-модель детали" && allDocuments[i].Name.Split().First() == currentObject.Name && allDocuments[i + 1].Type == "Файл")
                            {
                                currentObject.FileName = allDocuments[i + 1].FileName;
                                currentObject.FileId = allDocuments[i].LoodsmanId;

                                break;
                            }
                        }

                        string PathToExtractedFile = "";
                        if (string.IsNullOrEmpty(currentObject.FileName) != null)
                        {
                            PathToExtractedFile = ExtractFile(api, currentObject);

                            currentObject.PathToFile = PathToExtractedFile;
                            return currentObject;
                        }

                    }

                }

            }
            return null;
        }


        public string ExtractFile(IConnection api, LoodsmanObjectDTO ldo)
        {
            PathToLoodsmanDownloadFiles = @"\\waru16397\ElSh\CheckOuts\NT_ELECT#SPankratov\Temp\";

            object ErroreCore, ErroreMessage;
            api.MainSystem.GetFileById(ldo.LoodsmanId, ldo.FileName, "", out ErroreCore, out ErroreMessage);

            string PathToExtractedFile = $"{PathToLoodsmanDownloadFiles}{ldo.FileName}";
            if (System.IO.File.Exists(PathToExtractedFile))
            {
                return PathToExtractedFile;
            }
            return null;

        }
        public static DataTable ConvertToDataTable(byte[] info)
        {
            DataTable result = new DataTable();
            using (DataPacketReader dataPacketReader = new DataPacketReader(info))
                result.Load(dataPacketReader);
            return result;
        }

    }
}
