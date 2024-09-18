using SteamKit2;

namespace steamtoken
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                return;
            }

            var appid = args[0];
            var username = args[1];
            var password = args[2];

            var steamClient = new SteamClient();
            var manager = new CallbackManager(steamClient);
            var steamUser = steamClient.GetHandler<SteamUser>();
            var steamAuthTicket = steamClient.GetHandler<SteamAuthTicket>();
            var steamApp = steamClient.GetHandler<SteamApps>();

            var isRunning = true;
            var encryptedTicket = string.Empty;
            var appSessionTicket = string.Empty;

            manager.Subscribe<SteamClient.ConnectedCallback>(callback =>
            {
                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = username,
                    Password = password,
                });
            });

            manager.Subscribe<SteamUser.EncryptedAppTicketCallback>(callback =>
            {
                if (callback.Result != EResult.OK)
                {
                    isRunning = false;
                    Console.WriteLine("Failed to receive encrypted app ticket");
                    return;
                }

                encryptedTicket = Convert.ToBase64String(callback.Ticket);
            });

            manager.Subscribe<SteamUser.LoggedOnCallback>(async callback =>
            {
                if (callback.Result != EResult.OK)
                {
                    isRunning = false;
                    Console.WriteLine("Failed to log in to steam");
                    return;
                }

                var appTicket = await steamApp.GetAppOwnershipTicket((uint)int.Parse(appid));
                if (appTicket.Result != EResult.OK)
                {
                    await steamApp.RequestFreeLicense((uint)int.Parse(appid));
                }

                steamUser.GetEncryptedAppTicket((uint)int.Parse(appid));
                var appTicketByte = (await steamAuthTicket.GetAuthSessionTicket((uint)int.Parse(appid))).Ticket.ToArray();
                appSessionTicket = Convert.ToBase64String(appTicketByte);
                isRunning = false;
            });

            steamClient.Connect();

            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            steamUser.LogOff();

            if (!string.IsNullOrEmpty(appSessionTicket) && !string.IsNullOrEmpty(encryptedTicket))
            {
                Console.WriteLine($"{appSessionTicket}\n{encryptedTicket}");
            }
            else
            {
                Console.WriteLine("No token received");
            }
        }
    }
}