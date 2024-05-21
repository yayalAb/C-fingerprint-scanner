using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ReadFingerprintDemo;
using SourceAFIS.Simple;

class Program
{
    public readonly List<Person> _fingerprints;
    public int requestcount = 0;
    static async Task Main(string[] args)
    {
        await StartServerAsync();
    }

    static async Task StartServerAsync()
    {
        var listener = new HttpListener();
        var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName());

        foreach (var ipAddress in ipAddresses)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                listener.Prefixes.Add("http://" + ipAddress + ":8000/");
            }
        }
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Prefixes.Add("http://127.0.0.1:8000/");
        listener.Start();
        Console.WriteLine("Server started. Listening for connections...");
        try
        {
            while (true)
            {
                var context = await listener.GetContextAsync();
                Console.WriteLine("Client connected.");

                // Handle the request asynchronously
                _ = HandleRequestAsync(context);
            }
        }
        catch (OperationCanceledException)
        {
            // Do nothing - service is stopping
        }
        listener.Stop();
    }

    static async Task HandleRequestAsync(HttpListenerContext context)
    {
        DisplayNotificationClass notificationObj = new DisplayNotificationClass();
        AddFingerprint fp = new AddFingerprint();
        string accessToken = "";
        string authorizationHeader = context.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            accessToken = authorizationHeader.Substring("Bearer ".Length);
        }

        string clientAddress = context.Request.RemoteEndPoint.ToString();
        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
        context.Response.ContentType = "application/json, text/plain, */*";
        context.Response.AddHeader("Access-Control-Allow-Methods", "GET");
        context.Response.AddHeader("Access-Control-Allow-Headers", "*");
        context.Response.AddHeader("lang", "en");
        context.Response.AddHeader("Authorization", accessToken);
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        var queryParams = context.Request.QueryString;
        string UserId = queryParams.Get("Id");
        string Index = queryParams.Get("Index");
        bool isNewUser = false;
        bool CheckDuplication = false;
        bool idDesposRequest = false;

        if (queryParams?.Get("isNewUser") == "1")
        {
            isNewUser = true;
        }
        if (queryParams?.Get("WithCheck") == "1")
        {
            CheckDuplication = true;
        }
        if (queryParams?.Get("IsDespose") == "1")
        {
            idDesposRequest = true;
        }
        var response = new Response();
        if (context.Request.HttpMethod != "options" && (!string.IsNullOrEmpty(Index) && (!string.IsNullOrEmpty(Index))))
        {
            (string, string, int) result = fp.ScannFIngerPrint(UserId, Index, isNewUser, CheckDuplication, idDesposRequest);
            response = new Response
            {
                statusCode = result.Item3,
                success = true,
                message = result.Item2,
                FpImage = result.Item1
            };
        }
        else
        {
            response = new Response
            {
                statusCode = 200,
                success = true,
                message = "id and index must not be null",
                //  fpimage = fp.scannfingerprint()
            };

        }
        var json = JsonSerializer.Serialize(response);
        // Write response to output stream
        int retries = 3;
        bool success = false;
        while (!success && retries > 0)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                // await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                // Close output stream and context
                context.Response.OutputStream.Close();
                context.Response.Close();
                success = true;
            }
            catch (IOException ex)
            {

                notificationObj.displayNotfication($"IO Exception occurred: {ex.Message}");
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 64)
                {
                    notificationObj.displayNotfication("Network name is no longer available");
                    retries--;
                    if (retries == 0)
                    {
                        throw new Exception("Network name is no longer available!");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                notificationObj.displayNotfication($"Client disconnected: {clientAddress}");
            }
        }

    }
}
