namespace LoodsmanIcoUnloaderServer
{
    public  class MyLogger
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void CreateLogInfo(string message)
        {
            Logger.Info($"{message} {System.Security.Principal.WindowsIdentity.GetCurrent().Name}, время {DateTime.Now}");
        }
        public void CreateLogError(string message)
        {
            Logger.Error($"{message} {System.Security.Principal.WindowsIdentity.GetCurrent().Name}, время {DateTime.Now}");
        }
    }
}
