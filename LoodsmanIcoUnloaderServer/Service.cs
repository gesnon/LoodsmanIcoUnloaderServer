
using LoodsmanAPiServer;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LoodsmanIcoUnloaderServer
{
    public class Service
    {
        public static byte[] GetIco(LoodsmanObjectDTO ldo)
        {

            Bitmap newBitmap = GetFileBitmap(ldo);

            using (MemoryStream ms = new MemoryStream())
            {
                // Choose the format you want to save as. 
                // Using the original RawFormat is often best to maintain fidelity.
                //newBitmap.Save(ms, newBitmap.RawFormat);

                // Alternatively, you can specify a format like ImageFormat.Jpeg
                newBitmap.Save(ms, ImageFormat.Bmp);

                return ms.ToArray();
            }
        }


        public static void CreateTransitFile(string name)
        {
            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";

            if (File.Exists(pathTotransitFile))
            {
                File.Delete(pathTotransitFile);
            }
            File.WriteAllText(pathTotransitFile, $"{name}");
        }
        public static string GetNameFromFile(string name)
        {
            string pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string pathToDocuments = @"C:\Schneider\LoodsmanIcoUnloaderServer\TransitFiles\";

            string pathTotransitFile = $"{pathToDocuments}\\TransitDataForLoodsmanAPI.txt";
            if (File.Exists(pathTotransitFile))
            {
                string result =  File.ReadAllText(pathTotransitFile);

                File.Delete(pathTotransitFile);
                return result;
            }
            return "";
        }
        public static Bitmap GetFileBitmap(LoodsmanObjectDTO ldo)
        {
            string fileName = ldo.FileName;
            fileName = fileName.Remove(fileName.Length - 4, 4);

            // string inputFilesPath = outputPath;

            int width = 256;
            int height = 256;

            if (ldo.FileName.Contains(".m3d"))
            {
                string path = $@"{ldo.PathToFile}";

                Bitmap modernThumbnail = ThumbnailProvider.GenerateThumbnail($"{ldo.PathToFile}", width, height);


                FileInfo fileInfo = new FileInfo($"{path}");
                fileInfo.IsReadOnly = false;
                File.Delete($"{path}.m3d");
                if (modernThumbnail != null)
                {
                    return modernThumbnail;
                }

            }
            if (ldo.FileName.Contains(".a3d"))
            {
                string path = $@"{ldo.PathToFile}";

                Bitmap modernThumbnail = ThumbnailProvider.GenerateThumbnail($"{ldo.PathToFile}", width, height);


                FileInfo fileInfo = new FileInfo($"{path}");
                fileInfo.IsReadOnly = false;
                File.Delete($"{path}.a3d");
                if (modernThumbnail != null)
                {
                    return modernThumbnail;
                }

            }

            return null;

        }

        public class ThumbnailProvider
        {
            [ComImport]
            [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            internal interface IShellItemImageFactory
            {
                void GetImage(
                 [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
                 [In] uint flags,
                 [Out] out IntPtr phbm);
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SIZE
            {
                public int cx;
                public int cy;
            }

            [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern int SHCreateItemFromParsingName(
             [MarshalAs(UnmanagedType.LPWStr)] string path,
             IntPtr pbc,
             ref Guid riid,
             [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory factory);

            public static Bitmap GenerateThumbnail(string filePath, int width, int height)
            {
                try
                {
                    Guid iid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");
                    IShellItemImageFactory factory;
                    int hr = SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref iid, out factory);

                    if (hr != 0)
                        return null;

                    IntPtr hBitmap;
                    SIZE size = new SIZE { cx = width, cy = height };
                    factory.GetImage(size, 0x00000001, out hBitmap); // SIIGBF_THUMBNAILONLY

                    if (hBitmap == IntPtr.Zero)
                        return null;

                    return Bitmap.FromHbitmap(hBitmap);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
