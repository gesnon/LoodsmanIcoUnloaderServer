using LoodsmanAPiServer;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;

namespace LoodsmanIcoUnloaderServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        //MyLogger _myLogger = new MyLogger();


        private readonly ILogger<WeatherForecastController> _logger;
        private readonly MyLogger _myLogger;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, MyLogger myLogger)
        {
            _logger = logger;
            _myLogger = myLogger;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("TestGetMethod")]
        public string testMethod()
        {
            return "Тестовый метод отработал";

        }

        [HttpGet("GetPictures")]
        public IActionResult GetPictures(string LoodsmanObjectName)
        {
            _myLogger.CreateLogInfo($"Запрос для {LoodsmanObjectName}");

            //System.IO.File.AppendAllText(@"C:\Users\SPankratov\Desktop\Log.txt",$"Наименование {LoodsmanObjectName.Split('-').First()}" );

            Service.CreateTransitFile(LoodsmanObjectName.Split('-').First());
            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";

            string PathToXmlFile = @$"{pathToDocuments}\{LoodsmanObjectName.Split('-').First()}.xml";
            if (System.IO.File.Exists(PathToXmlFile))
            {
                System.IO.File.Delete(PathToXmlFile);
            }
            //if (System.IO.File.Exists($@"C:\{LoodsmanObjectName}.xml"))
            //{
            //    System.IO.File.Delete($@"C:\{LoodsmanObjectName}.xml");
            //}
            if (System.IO.File.Exists(pathTotransitFile))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();

                //p.StartInfo.FileName = @"C:\Users\SPankratov\source\repos\LoodsmanAPiServer\LoodsmanAPiServer\bin\Debug\LoodsmanAPiServer.exe";
                p.StartInfo.FileName = @"C:\Schneider\LoodsmanIcoUnloaderServer\LoodsmanAPiServer\bin\Debug\LoodsmanAPiServer.exe";

                p.Start();
            }

            LoodsmanObjectDTO resultLO = new LoodsmanObjectDTO();

            for (int i = 0; i < 10; i++)
            {
                //string test = @"C:\Users\SPankratov\Documents\15ГК.380.999.xml";
                if (System.IO.File.Exists(PathToXmlFile))
                {
                    XmlSerializer xmlDeserializer = new XmlSerializer(typeof(LoodsmanObjectDTO));
                    using (FileStream fs = new FileStream(PathToXmlFile, FileMode.OpenOrCreate))
                    {
                        LoodsmanObjectDTO deserializedCar = xmlDeserializer.Deserialize(fs) as LoodsmanObjectDTO;

                        if (deserializedCar != null)
                        {
                            resultLO = deserializedCar;
                        }

                        fs.Close();

                    }
                    break;
                }

                Thread.Sleep(500);
            }

            if (!string.IsNullOrEmpty(resultLO.Name))
            {

                if (resultLO.Error == "503")
                {
                    if (System.IO.File.Exists(PathToXmlFile))
                    {
                        FileInfo fileInfo2 = new FileInfo(PathToXmlFile);
                        fileInfo2.IsReadOnly = false;
                        System.IO.File.Delete(PathToXmlFile);
                    }
                    _myLogger.CreateLogError($"Для {LoodsmanObjectName} ошибка 503");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                if (resultLO.Error == "NotFound")
                {
                    if (System.IO.File.Exists(PathToXmlFile))
                    {
                        FileInfo fileInfo2 = new FileInfo(PathToXmlFile);
                        fileInfo2.IsReadOnly = false;
                        System.IO.File.Delete(PathToXmlFile);
                    }
                    _myLogger.CreateLogError($"Для {LoodsmanObjectName} ошибка 404");
                    return NotFound();
                }
                else
                {
                    byte[] result = Service.GetIco(resultLO);
                    if (System.IO.File.Exists(resultLO.PathToFile))
                    {
                        FileInfo fileInfo = new FileInfo(resultLO.PathToFile);
                        fileInfo.IsReadOnly = false;
                        System.IO.File.Delete(resultLO.PathToFile);

                        if (System.IO.File.Exists(PathToXmlFile))
                        {
                            FileInfo fileInfo2 = new FileInfo(PathToXmlFile);
                            fileInfo2.IsReadOnly = false;
                            System.IO.File.Delete(PathToXmlFile);
                        }
                    }
                    _myLogger.CreateLogInfo($"Для {LoodsmanObjectName} завершено успешно");
                    ClearTemporaryFolder();
                    return File(result, "image/bmp");
                }

            }

            return null;
        }

        public void ClearTemporaryFolder()
        {
            List<string> files = Directory.EnumerateFiles(@"C:\", "*.xml", SearchOption.TopDirectoryOnly).ToList();

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }


                System.IO.File.Delete(file);
            }
        }
    }
}
