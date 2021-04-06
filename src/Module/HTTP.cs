#nullable enable
using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;


namespace Eight.Module {
    public static class HTTP {
        public static LuaRegister[] HTTPLib = {
            new() {
                function = Request,
                name = "requestAsync"
            },
            new()
        };

        public static HttpClient Http = new();

        public static void Setup() {
            Runtime.LuaState.RequireF("http", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(HTTPLib);
            return 1;
        }

        public static int Request(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if ( !BIOS.biosConfig.EnableHTTP ) {
                state.PushBoolean(false);
                state.PushString("HTTP is disabled from BIOS");
                return 2;
            }

            string? body = null;
            string? method = null;

            var url = state.CheckString(1);
            if ( !state.IsNoneOrNil(2) ) state.CheckString(2);
            if ( !state.IsNoneOrNil(3) ) state.CheckString(3);
            if ( !state.IsNoneOrNil(4) ) state.CheckString(4);

            if ( state.IsString(2) ) body = state.ToString(2);

            if ( state.IsString(4) ) method = state.ToString(4);

            var headers = new Dictionary<string, string>();

            if ( state.IsTable(3) ) {
                state.PushNil();

                while ( state.Next(3) ) {
                    string value = state.ToString(-1);
                    string key = state.ToString(-2);

                    headers[key] = value;

                    state.Pop(1);
                }
            }

            Random random = new();
            var id = random.Next().ToString("x8");

            Uri? uri;
            var ok = Uri.TryCreate(url, UriKind.Absolute, out uri);

            if ( ok && uri != null ) {
                Request(uri, body, headers, method, id);
                state.PushBoolean(true);
                state.PushString(id);
            } else {
                state.PushBoolean(false);
                state.PushString("Malformed URI");
            }

            return 2;
        }

        public static async void Request(Uri url, string? body,
            Dictionary<string, string>? headers, string? method, string rnd) {
            HttpRequestMessage message = new() {
                RequestUri = url
            };

            if ( body != null )
                message.Content = new StringContent(body);

            if ( headers != null )
                foreach ( var (key, value) in headers )
                    message.Headers.TryAddWithoutValidation(key, value);

            message.Method = method != null ? new HttpMethod(method) : HttpMethod.Get;

            try {
                var response = await Http.SendAsync(message);

                ProcessHttpResponse(response, rnd);
            } catch ( Exception e ) {
                Utils.LuaParameter[] parameters = {
                    new() {
                        Type = LuaType.String,
                        Value = "http_failure"
                    },
                    new() {
                        Type = LuaType.String,
                        Value = rnd
                    },
                    new() {
                        Type = LuaType.String,
                        Value = e.Message
                    }
                };

                Event.Push(parameters);
            }
        }

        private static async void ProcessHttpResponse(HttpResponseMessage response, string rnd) {
            var stream = await response.Content.ReadAsStreamAsync();
            var content = ReadToEnd(stream);

            Utils.LuaParameter[] parameters = {
                new() {
                    Type = LuaType.String,
                    Value = "http_success"
                },
                new() {
                    Type = LuaType.String,
                    Value = rnd
                },
                new() {
                    Type = LuaType.String,
                    Value = content
                }
            };

            Event.Push(parameters);
        }

        public static byte[] ReadToEnd(Stream stream) {
            long originalPosition = 0;

            if ( stream.CanSeek ) {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ( (bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0 ) {
                    totalBytesRead += bytesRead;

                    if ( totalBytesRead == readBuffer.Length ) {
                        int nextByte = stream.ReadByte();
                        if ( nextByte != -1 ) {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if ( readBuffer.Length != totalBytesRead ) {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            } finally {
                if ( stream.CanSeek ) {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}