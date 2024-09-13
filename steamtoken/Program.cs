using SteamKit2;

namespace steamtoken;

class Program
{
    static void Main(string[] args)
    {
        var appid = args[0];
        var username = args[1];
        var password = args[2];
        var steamClient = new SteamClient();
        var manager = new CallbackManager(steamClient);
        var steamUser = steamClient.GetHandler<SteamUser>();
        var steamAuthTicket = steamClient.GetHandler<SteamAuthTicket>();
        var isRunning = true;
        byte[]? token = null;

        manager.Subscribe<SteamClient.ConnectedCallback>(callback =>
        {
            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password
            });
        });

        manager.Subscribe<SteamUser.LoggedOnCallback>(callback =>
        {
            if (callback.Result != EResult.OK)
            {
                isRunning = false;
                Console.WriteLine("Failed to log in to steam");
            }

            token = steamAuthTicket.GetAuthSessionTicket((uint)int.Parse(appid)).Result.Ticket.ToArray();
            isRunning = false;
        });

        steamClient.Connect();

        while (isRunning)
        {
            manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
        }

        steamUser.LogOff();
        if (token != null)
        {
            string hexString = BitConverter.ToString(token).Replace("-", " ");
            Console.WriteLine(hexString);
        }
        else
        {
            Console.WriteLine("No token received");
        }
    }
}