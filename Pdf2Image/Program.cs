
// xx.exe path/to/file.pdf path/to/image.png
using ImageMagick;
using Pdf2Image;
using System.Drawing.Imaging;

string[] arguments = args;

try
{
    if (arguments.Length < 2)
    {
        throw new("Please enter path to pdf and path of image");
    }

    string pathPDF = arguments[0];
    string pathImage = arguments[1];

    Console.WriteLine("PDF source file read from: " + pathPDF);
    Console.WriteLine("Image target files save to: " + pathImage);

    ImageFormat format = Utility.GetImageFormat(pathImage);
    string filename = Path.GetFileName(pathImage);
    string? directory = Path.GetDirectoryName(pathImage);

    var list = Utility.Pdf2Image(pathPDF, format);

    Console.WriteLine("-------------------");
    for (int i = 0; i < list.Count; i++)
    {

        using (MemoryStream ms = list[i])
        {
            string path = pathImage;
            var filenames = new List<string>(filename.Split("."));
            filenames[^2] = filenames[^2] + "_" + ($"{i + 1:00}");
            path = directory + "\\" + string.Join(".", filenames);

            Utility.Stream2File(ms, path);
            Console.WriteLine("Saved {0}", path);
        }
    }

    Console.WriteLine("Done");
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}

