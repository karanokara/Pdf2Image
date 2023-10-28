using System;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using System.Drawing.Imaging;
using ImageMagick;
using System.Reflection;

namespace Pdf2Image
{
    public class Utility
    {
        /// <summary>
        /// convert PDF pages to png using GhostScript dll
        /// 
        /// using dpi 192, normally to the same size of the page in pdf
        /// 
        /// https://github.com/jhabjan/Ghostscript.NET/blob/master/Ghostscript.NET.Samples/Samples/RasterizerSample1.cs
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <param name="imageFormat"></param>
        /// <returns></returns>
        public static List<MemoryStream> Pdf2Image(string pdfPath, ImageFormat imageFormat)
        {
            int desired_dpi = 192;
            List<MemoryStream> mss = new();
            byte[]? library = GetEmbededResource("Pdf2Image.Resource.gsdll64.dll");
            //string dllPath = ".\\Resource\\gsdll64.dll";
            //GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(dllPath);

            if (library == null) throw new FileNotFoundException("No gsdll library");

            using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
            {
                //rasterizer.CustomSwitches.Add("-dUseCropBox");
                //rasterizer.CustomSwitches.Add("-c");
                //rasterizer.CustomSwitches.Add("[/CropBox [24 72 559 794] /PAGES pdfmark");
                //rasterizer.CustomSwitches.Add("-f");

                //rasterizer.Open(pdfPath, gvi, false);
                rasterizer.Open(pdfPath, library);

                for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
                {
                    System.Drawing.Image img = rasterizer.GetPage(desired_dpi, pageNumber);
                    //string pageFilePath = Path.Combine(outputPath, "Page-" + pageNumber.ToString() + ".png");
                    //img.Save(pageFilePath, ImageFormat.Png);

                    MemoryStream ms = new();
                    img.Save(ms, imageFormat);  // don't need to be windows                    
                    ms.Position = 0;        // reset cursor to the start point

                    // compress a little bit
                    var optimizer = new ImageOptimizer();
                    optimizer.LosslessCompress(ms);

                    mss.Add(ms);
                }
            }
            return mss;
        }

        /// <summary>
        /// Write memory binary stream to a file
        /// </summary>
        /// <param name="st"></param>
        /// <param name="filename"></param>
        public static void Stream2File(MemoryStream ms, string filename)
        {
            // FileMode.Create will empty out existing file at first
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
            {
                ms.WriteTo(fs);     // using WriteTo() for MemoryStream will write from start point
            }
        }

        public static byte[]? GetEmbededResource(string resourceName)
        {
            // Specify the name of the embedded resource (namespace + filename)
            //string resourceName = "YourNamespace.YourEmbeddedDll.dll";

            // Load the embedded .dll file as binary data
            var assembly = Assembly.GetExecutingAssembly();
            Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null)
            {
                Console.WriteLine("Embedded resource not found.");
                return null;
            }

            // Read the binary data into a byte array
            byte[] dllBytes = new byte[resourceStream.Length];
            resourceStream.Read(dllBytes, 0, dllBytes.Length);

            // At this point, dllBytes contains the binary data of the embedded .dll file

            // Now, you can do whatever you need with the binary data, such as saving it to a file
            //File.WriteAllBytes("YourEmbeddedDll.dll", dllBytes);

            //Console.WriteLine("Embedded .dll file saved to disk.");
            return dllBytes;
        }

        public static ImageFormat GetImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    string.Format("Unable to determine file extension for fileName: {0}", fileName));

            switch (extension.ToLower())
            {
                case @".bmp":
                    return ImageFormat.Bmp;

                case @".gif":
                    return ImageFormat.Gif;

                case @".ico":
                    return ImageFormat.Icon;

                case @".jpg":
                case @".jpeg":
                    return ImageFormat.Jpeg;

                case @".png":
                    return ImageFormat.Png;

                case @".tif":
                case @".tiff":
                    return ImageFormat.Tiff;

                case @".wmf":
                    return ImageFormat.Wmf;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Set the compression quality (0-100, where 100 is the best quality)
        /// 
        /// Will result bigger file!!
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static MemoryStream CompressImage(Stream inputImage, int quality)
        {
            MemoryStream ms = new MemoryStream();
            using (MagickImage image = new MagickImage(inputImage))
            {
                // Set the compression quality (0-100, where 100 is the best quality)
                image.Quality = quality;

                // Write the compressed image to the output file
                image.Write(ms);
            }
            return ms;
        }
    }
}
