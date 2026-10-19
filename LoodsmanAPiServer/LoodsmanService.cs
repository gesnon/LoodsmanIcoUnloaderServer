using Ascon.Plm.DataPacket;
using Ascon.Plm.ServerApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LoodsmanAPiServer
{
    public class LoodsmanService
    {
        string PathToLoodsmanDownloadFiles = "";

        public string GetObjectNameFromFile()
        {
            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";

            if (File.Exists(pathTotransitFile))
            {
                string result = File.ReadAllText(pathTotransitFile);

                File.Delete(pathTotransitFile);
                return result;
            }
            return "";
        }

        public LoodsmanObjectDTO GetPath()
        {

            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";

            string Name = GetObjectNameFromFile();

            Name = Name.Split('-')[0];

            LoodsmanObjectDTO ErrorObject = new LoodsmanObjectDTO { Name = Name };

            if (Name == "")
            {
                LoodsmanObjectDTO res = new LoodsmanObjectDTO();
                res.Name = "не получилось получить имя";
                return res;
            }
            ConnectionFactory _connectionFactory = new ConnectionFactory();
            using (var api = _connectionFactory.CreateConnection("waru16397", 8076))
            {
                object apiErrorCode, apiErrorMessage;
                api.MainSystem.ConnectToDBEx("ElSh", "Администратор", "uDSb5MFT9ZsnYPKc6EfR", out apiErrorCode, out apiErrorMessage);//Подключаемся к БД, логин и пароль можно оставить пустыми для Win-входа

                if (apiErrorMessage != null)
                {

                    ErrorObject.Name = Name;
                    ErrorObject.Error = "503";
                    return ErrorObject;

                }
                else
                {
                    string CheckDBName = api.MainSystem.CurrentBase.ToString();
                    object ErrorObjectCode;
                    object ErrorObjectMessage;
                    byte[] ObjectsFromLoodsman = (byte[])api.MainSystem.GetVersionList("Деталь", Name, out ErrorObjectCode, out ErrorObjectMessage);
                    DataTable ObjectsFromLoodsmanDT = ConvertToDataTable(ObjectsFromLoodsman);

                    List<LoodsmanObjectDTO> loodsmanObjectDTOs = LoodsmanObjectDTO.CreateList(ObjectsFromLoodsmanDT);

                    List<LoodsmanObjectDTO> onlyApprovedObjects = loodsmanObjectDTOs.Where(_ => _.State == "Утвержден").ToList();



                    object ErrorObjectCodeSbor, ErrorObjectMessageSbor;
                    byte[] SborkiFromLoodsman = (byte[])api.MainSystem.GetVersionList("Сборочная единица", Name, out ErrorObjectCodeSbor, out ErrorObjectMessageSbor);
                    DataTable ObjectsFromLoodsmanDTSbor = ConvertToDataTable(SborkiFromLoodsman);
                    List<LoodsmanObjectDTO> loodsmanObjectDTOsSbor = LoodsmanObjectDTO.CreateList(ObjectsFromLoodsmanDTSbor);
                    List<LoodsmanObjectDTO> onlyApprovedObjectsSbor = loodsmanObjectDTOsSbor.Where(_ => _.State == "Утвержден").ToList();

                    if (onlyApprovedObjects.Count == 0 && onlyApprovedObjectsSbor.Count == 0)
                    {
                        ErrorObject.Name = Name;
                        ErrorObject.Error = "NotFound";
                        return ErrorObject;
                        //return new LoodsmanObjectDTO { Name = "NotFound" };
                    }
                    //if (onlyApprovedObjectsSbor.Count == 0)
                    //{
                    //    ErrorObject.Name = Name;
                    //    ErrorObject.Error = "NotFound";
                    //    return ErrorObject;
                    //    //return new LoodsmanObjectDTO { Name = "NotFound" };
                    //}

                    else
                    {
                        //LoodsmanObjectDTO currentObject = onlyApprovedObjects.OrderByDescending(_ => _.Version).First();

                        LoodsmanObjectDTO currentObject = new LoodsmanObjectDTO();

                        if (onlyApprovedObjects.Count != 0)
                        {
                            currentObject = onlyApprovedObjects.OrderByDescending(_ => _.Version).First();
                        }
                        if (onlyApprovedObjectsSbor.Count != 0)
                        {
                            currentObject = onlyApprovedObjectsSbor.OrderByDescending(_ => _.Version).First();
                        }

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
                                string Version1 = documentsTable.Rows[i]["_VERSION"].ToString();
                                int Id1 = (int)documentsTable.Rows[i]["_ID_VERSION"];
                                allDocuments.Add(new LoodsmanObjectDTO { Name = Name1, Type = Type1, LoodsmanId = Id1, Version = Version1 });
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                        }

                        if (currentObject.Type == "Деталь")
                        {
                            for (int i = 0; i < allDocuments.Count; i++)
                            {

                                if (allDocuments[i].Type == "3D-модель детали" && allDocuments[i].Name.Split().First() == currentObject.Name && allDocuments[i + 1].Type == "Файл")
                                {
                                    currentObject.FileName = allDocuments[i + 1].FileName;
                                    currentObject.FileId = allDocuments[i].LoodsmanId;

                                    break;
                                }
                            }
                        }
                        if (currentObject.Type == "Сборочная единица")
                        {
                            for (int i = 0; i < allDocuments.Count; i++)
                            {

                                if (allDocuments[i].Type == "3D-модель сборки" && allDocuments[i].Name.Split().First() == currentObject.Name && allDocuments[i + 1].Type == "Файл")
                                {
                                    currentObject.FileName = allDocuments[i + 1].FileName;
                                    currentObject.FileId = allDocuments[i].LoodsmanId;

                                    break;
                                }
                            }
                        }


                        string PathToExtractedFile = "";
                        if (string.IsNullOrEmpty(currentObject.FileName) != null)
                        {
                            if (currentObject.FileId == 0)
                            {
                                ErrorObject.Name = Name;
                                ErrorObject.Error = "NotFound";
                                return ErrorObject;
                            }
                            else
                            {
                                PathToExtractedFile = ExtractFile(api, currentObject);

                                currentObject.PathToFile = PathToExtractedFile;
                                if (File.Exists(pathTotransitFile))
                                {
                                    //  File.Delete(pathTotransitFile);
                                }

                                return currentObject;
                            }


                        }


                    }

                }

            }
            return null;
        }

        public string ExtractFile(IConnection api, LoodsmanObjectDTO ldo)
        {
            PathToLoodsmanDownloadFiles = @"\\waru16397\ElSh\CheckOuts\Администратор\Temp\";
            //PathToLoodsmanDownloadFiles = @"\\waru16397\ElSh\CheckOuts\NT_ELECT#SPankratov\Temp\";

            object ErroreCore, ErroreMessage;
            api.MainSystem.GetFileById(ldo.FileId, ldo.FileName, "", out ErroreCore, out ErroreMessage);

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
