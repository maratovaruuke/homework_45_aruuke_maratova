using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using RazorEngine;
using RazorEngine.Templating;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace homework_45_aruuke_maratova
{
    internal class Server
    {
        private Thread _serverThread;
        private string _siteDirectory;
        private HttpListener _listener;
        private int _port;

        JsonSerializerOptions options = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        public Server(string path, int port)
        {
            Initialize(path, port);
        }

        private void Initialize(string path, int port)
        {
            _siteDirectory = path;
            _port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }

        //private void Stop()
        //{
        //    _serverThread.Abort();
        //    _listener.Stop();
        //}

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    Process(_listener.GetContext());
                }
                catch
                {
                    //Stop();
                    Console.WriteLine("Error");
                }
            }
        }

        private void Process(HttpListenerContext httpListenerContext)
        {
            //httpListenerContext.Request.HttpMethod

            string fileName = httpListenerContext.Request.Url.AbsolutePath;
        
            fileName = fileName.Substring(1);
 
            fileName = Path.Combine(_siteDirectory, fileName);
            Console.WriteLine(fileName);
            string content = "";
            if (File.Exists(fileName))
            {
                if (fileName.Contains("html"))
                {
                    content = BuildHtml(fileName, httpListenerContext);
                }
                else
                {
                    content = File.ReadAllText(fileName);
                }
                try
                {
                    byte[] htmlbytes = System.Text.Encoding.UTF8.GetBytes(content);
                    Stream fileStream = new MemoryStream(htmlbytes);
                    httpListenerContext.Response.ContentType = GetContentType(fileName);
                    httpListenerContext.Response.ContentLength64 = fileStream.Length;
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.OK;

                    int dataLength;
                    do
                    {
                        dataLength = fileStream.Read(htmlbytes, 0, htmlbytes.Length);
                        httpListenerContext.Response.OutputStream.Write(htmlbytes, 0, dataLength);
                    } while (dataLength > 0);
                    fileStream.Close();
                    httpListenerContext.Response.OutputStream.Flush();
                }
                catch
                {
                    httpListenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                httpListenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            httpListenerContext.Response.OutputStream.Close();
        }

        private string? GetContentType(string fileName)
        {
            var dictionary = new Dictionary<string, string> {

            {".css",  "text/css"},

            {".html", "text/html"},

            {".ico",  "image/x-icon"},

            {".js",   "application/x-javascript"},

            {".json", "application/json"},

            {".png",  "image/png"}

        };



            string contentType = "";

            string fileExtension = Path.GetExtension(fileName);

            dictionary.TryGetValue(fileExtension, out contentType);

            return contentType;
        }
        private string BuildHtml(string fileName, HttpListenerContext context)
        {
            string html = "";
            string layoutPath = Path.Combine(_siteDirectory, "layout.html");
            var query = context.Request.QueryString;
            var razorService = Engine.Razor;

            if (!razorService.IsTemplateCached(razorService.GetKey("layout"), null))
                razorService.AddTemplate(razorService.GetKey("layout"), File.ReadAllText(layoutPath));
            if (!razorService.IsTemplateCached(razorService.GetKey(fileName), null))
            {
                razorService.AddTemplate(razorService.GetKey(fileName), File.ReadAllText(fileName));
                razorService.Compile(razorService.GetKey(fileName));
            }
            string json = File.ReadAllText("../../../employees.json");

            List<Employee> employees;

            employees = JsonSerializer.Deserialize<List<Employee>>(File.ReadAllText("../../../employees.json"));

            if(query.HasKeys())
            {
                int IdFrom = Convert.ToInt32(query.Get("IdFrom"));
                int IdTo = Convert.ToInt32(query.Get("IdTo"));
                Console.WriteLine("IdFrom" + IdFrom);
                Console.WriteLine("IdTo" + IdTo);
                employees = employees.Where(x => x.Id >= IdFrom && x.Id <= IdTo).ToList();
            }

            html = razorService.Run(razorService.GetKey(fileName), null, new
            {
                Title = "Hello World!",
                X = 1,
                Text = "Main Page",
                Employees = employees
            });
            return html;
        }
    }   
}
