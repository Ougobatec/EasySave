// See https://aka.ms/new-console-template for more information
using EasySave;
using EasySave.DataManipulation;


namespace EasySave
{
    class Program
    {
        // Création d'un délégué
        delegate void MonEvenementHandler();

        // Création d'un événement basé sur le délégué
        static event MonEvenementHandler QuandQuelqueChoseSePasse;

        // Fonction qui sera exécutée quand l'événement est déclenché
        static void Reaction()
        {
            // Affiche un message indiquant que l'événement a eu lieu
            Console.WriteLine("L'événement a eu lieu !");
        }

        static void Main(string[] args)
        {
            // Abonne la méthode Reaction à l'événement QuandQuelqueChoseSePasse
            QuandQuelqueChoseSePasse += Reaction;

            // Déclenche l'événement, ce qui appelle la méthode Reaction
            QuandQuelqueChoseSePasse?.Invoke();

            // Affiche un message de bienvenue
            Console.WriteLine("Hello, World!");

            // Création d'une instance de Singleton
            Singleton monsigleton = new Singleton();

            // Utilisation du logger pour enregistrer des messages d'information
            monsigleton.MonLogger.Info("eee");
            monsigleton.MonLogger.Info("ssssssssssseee");
            monsigleton.MonLogger.Info("ssssssssssseee");

            // Utilisation du logger pour enregistrer un message d'erreur
            monsigleton.MonLogger.Error("ssssscazefdzefzefzefezfzefsssssseee");

            // Utilisation du logger pour enregistrer un message d'avertissement
            monsigleton.MonLogger.Warning("ssssssssssseee");

            // Utilisation du logger pour enregistrer un message de trace
            monsigleton.MonLogger.Trace("ssssssssssseee");

            // Utilisation du logger pour enregistrer un message de débogage
            monsigleton.MonLogger.Debug("ssssssssssseee");

            // Exemple de copie de données (commenté)
            // DataManipulation.Copy();

            // Création d'un délégué et exécution de la méthode DireBonjour (commenté)
            // Testdelegate.MonDelegate monAction = new Testdelegate.MonDelegate(new Testdelegate().DireBonjour);
            // Testdelegate.AfficherMessage(Testdelegate.DireQuelqueChose);
        }
    }

}
