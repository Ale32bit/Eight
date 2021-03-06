﻿using DiscordRPC;
using System;

namespace Eight {
    class Discord {
        public static DiscordRpcClient Client;
        public static readonly string ClientID = "822815777664794634";

        private static void Log(string message) {
            Console.WriteLine($"[RPC] {message}");
        }

        public static void Init() {
            Client = new(ClientID) {
                SkipIdenticalPresence = true,
                Logger = new DiscordRPC.Logging.ConsoleLogger(DiscordRPC.Logging.LogLevel.Error)
            };

            Client.OnReady += (sender, e) => {
                Log($"Ready: {e.User.Username}");
            };

            Client.Initialize();
        }

        public static void SetStatus(string details, string state = "") {
            if ( Client != null && !Client.IsDisposed ) {
                Client.SetPresence(new RichPresence() {
                    Details = details,
                    State = state,
                    Assets = new() {
                        LargeImageKey = "main",
                        LargeImageText = $"Eight {Eight.Version}",
                    },
                });

                Client.UpdateStartTime();
            }
        }

        public static void Clear() {
            if ( Client != null && !Client.IsDisposed ) {
                Client?.ClearPresence();
            }
        }

        public static void Dispose() {
            Clear();
            Client?.Dispose();
        }

    }
}
