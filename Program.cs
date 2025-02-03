// See https://aka.ms/new-console-template for more information
using EasySave;



namespace EasySave
{
    class Program
    {
        static void Main(string[] args)

        {


            Console.WriteLine("Hello, World!");




            Singleton monsigleton = new Singleton();


            monsigleton.MonLogger.Info("eee");

            monsigleton.MonLogger.Info("ssssssssssseee");

            monsigleton.MonLogger.Info("ssssssssssseee");

            monsigleton.MonLogger.Error("ssssscazefdzefzefzefezfzefsssssseee");

            monsigleton.MonLogger.Warning("ssssssssssseee");

            monsigleton.MonLogger.Trace("ssssssssssseee");

            monsigleton.MonLogger.Debug("ssssssssssseee");



        }




    }

}
