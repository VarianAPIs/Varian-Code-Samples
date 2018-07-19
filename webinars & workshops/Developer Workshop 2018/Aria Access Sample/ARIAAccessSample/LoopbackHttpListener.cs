using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace DevWorkshop2018AriaAcess
{
	public class LoopbackHttpListener : IDisposable
    {
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)
        HttpListener _httpListener;
        TaskCompletionSource<string> _source = new TaskCompletionSource<string>();

        public LoopbackHttpListener(string path = null)
        {
            var url = path.EndsWith("/") ? path : $"{path}/";
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();
            Configure();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);
                _httpListener.Close();
                _httpListener.Stop();
            });
        }

        async Task Configure()
        {
            var ctx = await _httpListener.GetContextAsync();

            if (ctx.Request.HttpMethod == "GET")
            {
                SetResult(ctx.Request.QueryString.GetValues(0)[0], ctx);
            }
            else if (ctx.Request.HttpMethod == "POST")
            {
                if (!ctx.Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.StatusCode = 415;
                }
                else
                {
                    using (var sr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
                    {
                        var body = await sr.ReadToEndAsync();
                        SetResult(body, ctx);
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = 405;
            }
        }

        private void SetResult(string value, HttpListenerContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                var data = Encoding.UTF8.GetBytes("<body><h1>Sign-in completed. You can now return to the application.</h1></body>");
                ctx.Response.OutputStream.Write(data, 0, data.Length);
                ctx.Response.OutputStream.Close();
                _source.TrySetResult(value);
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                var data = Encoding.UTF8.GetBytes("<h1>Invalid request.</h1>");
                ctx.Response.OutputStream.Write(data, 0, data.Length);
                ctx.Response.OutputStream.Close();
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000);
                _source.TrySetCanceled();
            });
            return _source.Task;
        }
    }
}
