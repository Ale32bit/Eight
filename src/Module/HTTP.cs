#nullable enable
using KeraLua;
using System;
using System.Collections.Generic;
using System.Net.Http;
using static Eight.Utils;

namespace Eight.Module {
    public static class HTTP {
        public static LuaRegister[] AudioLib = {
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
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.NewLib(AudioLib);
            return 1;
        }

        public static int Request(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            string? body = null;
            string? method = null;

            state.ArgumentCheck(state.IsString(1), 1, "expected string");
            state.ArgumentCheck(state.IsString(2) || state.IsNoneOrNil(2), 2, "expected string");
            state.ArgumentCheck(state.IsTable(3) || state.IsNoneOrNil(3), 3, "expected string");
            state.ArgumentCheck(state.IsString(4) || state.IsNoneOrNil(4), 4, "expected string");

            if (state.IsString(2)) body = state.ToString(2);

            if (state.IsString(4)) method = state.ToString(4);

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

            Random random = new();
            var id = random.Next().ToString("x8");

            Uri? uri;
            var ok = Uri.TryCreate(url, UriKind.Absolute, out uri);

            if (ok && uri != null) {
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

            if (body != null)
                message.Content = new StringContent(body);

            if (headers != null)
                foreach (var (key, value) in headers)
                    message.Headers.TryAddWithoutValidation(key, value);

            message.Method = method != null ? new HttpMethod(method) : HttpMethod.Get;

            try {
                var response = await Http.SendAsync(message);

                ProcessHttpResponse(response, rnd);
            } catch (Exception e) {
                LuaParameter[] parameters = {
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

                Eight.PushEvent(parameters);
            }
        }

        private static async void ProcessHttpResponse(HttpResponseMessage response, string rnd) {
            var content = await response.Content.ReadAsByteArrayAsync();

            LuaParameter[] parameters = {
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

            Eight.PushEvent(parameters);
        }
    }
}