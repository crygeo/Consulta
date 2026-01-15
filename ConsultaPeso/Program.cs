using ConsultaPeso;
using ConsultaPeso.Application;
using ConsultaPeso.Infrastructure.Api;

internal static class Program
{
    
    private static Mutex? _mutex;

    [STAThread]
    static void Main()
    {
        const string mutexName = @"Global\ConsultaPesosApp";
        _mutex = new Mutex(true, mutexName, out bool isFirstInstance);

        if (!isFirstInstance)
        {
            // Ya hay una instancia corriendo → salir
            return;
        }
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            ReiniciarAplicacion(e.Exception);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            ReiniciarAplicacion(e.ExceptionObject as Exception);
        };
        

        // HttpClient único (muy importante)
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://mock.api.local"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        // Infraestructura
        var apiClient = new ApiClient(httpClient);

        // Application
        var consultaService = new ConsultaPesosService(apiClient);

        // UI
        Application.Run(new MainForm(consultaService));

        _mutex?.ReleaseMutex();

    }
    
    private static void ReiniciarAplicacion(Exception? ex)
    {
        try
        {
            var now = DateTime.Now;

            var logPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "crash.log");

            File.AppendAllText(logPath,
                $"{now}: {ex}\n");

            // Evitar loop de reinicio
            var lastCrashFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "last_crash.txt");


            if (File.Exists(lastCrashFile))
            {
                var last = DateTime.Parse(File.ReadAllText(lastCrashFile));
                if ((now - last).TotalSeconds < 10)
                    return; // aborta reinicio
            }

            File.WriteAllText(lastCrashFile, now.ToString("O"));

            Application.Restart();
        }
        finally
        {
            Environment.Exit(0);
        }
    }


}