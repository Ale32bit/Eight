#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using KeraLua;
using static Eight.Utils;
using Lua = KeraLua.Lua;

namespace Eight.LuaLibs {
    public static class HTTP {
        public static LuaRegister[] HTTP_Lib = {
            new() {
                function = Request,
                name = "request"
            },
            new(),
        };

        public static HttpClient Http = new();

        public static void Setup() {
            Logic.Lua.LuaState.RequireF("http", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(HTTP_Lib);
            return 1;
        }

        public static int Request(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            string? body = null;
            string? method = null;


            if (!state.IsString(1)) {
                state.Error(GenArgError(1, state.TypeName(1), "string", "nil"));
                return 0;
            }

            if (state.IsString(2)) {
                body = state.ToString(2);
            }
            else if (!state.IsNoneOrNil(2)) {
                state.Error(GenArgError(2, state.TypeName(2), "string", "nil"));
                return 0;
            }

            if (state.IsString(4)) {
                method = state.ToString(4);
            }
            else if (!state.IsNoneOrNil(4)) {
                state.Error(GenArgError(4, state.TypeName(4), "string", "nil"));
                return 0;
            }

            var url = state.ToString(1);

            var headers = new Dictionary<string, string>();

            if (state.IsTable(3)) {
                state.PushNil();

                while (state.Next(3)) {
                    string value = state.ToString(-1);
                    string key = state.ToString(-2);

                    headers[key] = value;

                    state.Pop(1);
                }
            }
            else if (!state.IsNoneOrNil(3)) {
                state.Error(GenArgError(3, state.TypeName(3), LuaType.Table.ToString()));
                return 0;
            }

            Random random = new();
            var id = random.Next().ToString("x8");

            Uri? uri;
            bool ok = Uri.TryCreate(url, UriKind.Absolute, out uri);

            if (ok && uri != null) {
                Request(uri, body, headers, method, id);
                state.PushBoolean(true);
                state.PushString(id);
            }
            else {
                state.PushBoolean(false);
                state.PushString("Malformed URI");
            }

            return 2;
        }

        public static async void Request(Uri url, string? body,
            Dictionary<string, string>? headers, string? method, string rnd) {
            HttpRequestMessage message = new() {
                RequestUri = url,
            };

            if (body != null)
                message.Content = new StringContent(body);

            if (headers != null) {
                foreach (var (key, value) in headers) {
                    message.Headers.TryAddWithoutValidation(key, value);
                }
            }
            
            message.Method = method != null ? new HttpMethod(method) : HttpMethod.Get;

            try {
                var response = await Http.SendAsync(message);

                ProcessHttpResponse(response, rnd);
            }
            catch (Exception e) {
                LuaParameter[] parameters = {
                    new() {
                        Type = LuaType.String,
                        Value = "http_failure",
                    },
                    new() {
                        Type = LuaType.String,
                        Value = rnd,
                    },
                    new() {
                        Type = LuaType.String,
                        Value = e.Message,
                    }
                };

                Eight.PushEvent(parameters);
            }
        }

        private static async void ProcessHttpResponse(HttpResponseMessage response, string rnd) {
            var content = await response.Content.ReadAsByteArrayAsync();

            LuaParameter[] parameters = {
                new() {
                    Type = LuaType.String,
                    Value = "http_success",
                },
                new() {
                    Type = LuaType.String,
                    Value = rnd,
                },
                new() {
                    Type = LuaType.String,
                    Value = content,
                }
            };

            Eight.PushEvent(parameters);
        }
    }
}