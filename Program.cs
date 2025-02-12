using EasySave.Views;

namespace EasySave
{
    class Program
    {
        static void Main()
        {
            ViewCLI view = new();
            view.Run();
        }
    }
}

public static string GetFolderPath(Environment.SpecialFolder folder);

Console.WriteLine(HashFromFile(new FileInfo(newPath)));


public static string GetTempPath();
string result = Path.GetTempPath();
Console.WriteLine(result);


static string HashFromFile(FileInfo file)
{
    byte[] firstHash = MD5.Create().ComputeHash(file.OpenRead());
    return Encoding.Default.GetString(firstHash);
}
