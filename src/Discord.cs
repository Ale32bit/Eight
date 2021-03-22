using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPC;

namespace Eight {
    class Discord {
        public static DiscordRpcClient Client;

        private static void Log(string message) {
            Console.WriteLine($"[RPC] {message}");
        }

        public static void Init() {
            Client = new("822815777664794634");

            Client.Logger = new DiscordRPC.Logging.ConsoleLogger() {
                Level = DiscordRPC.Logging.LogLevel.Warning
            };

            Client.OnReady += (sender, e) => {
                Log($"Ready: {e.User.Username}");
            };

            Client.OnPresenceUpdate += (sender, e) => {
                Log($"Update: {e.Presence}");
            };

            Client.Initialize();
        }


        public static void SetStatus(string details, string state) {
            Client.SetPresence(new RichPresence() {
                Details = details,
                State = state,
                Assets = new Assets() {
                    LargeImageKey = "main",
                    LargeImageText = $"Eight {Eight.Version}",
                }
            });
        }

    }
}
