using System;

namespace EasySave
{
    public sealed class Singleton
    {
        private static readonly Lazy<Singleton> instance = new Lazy<Singleton>(() => new Singleton());
        public Logger MonLogger { get; private set; }

        // Déclaration de l'événement
        public event EventHandler<string> LogEvent;

        private Singleton()
        {
            MonLogger = new Logger("log.json");

            // Abonnement à l'événement du logger
            MonLogger.LogWritten += (sender, log) =>
            {
                Console.WriteLine($"[Event Capturé] Nouveau Log: {log}");
                LogEvent?.Invoke(this, log);
            };
        }

        public static Singleton Instance => instance.Value;
    }
}
