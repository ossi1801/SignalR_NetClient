using Microsoft.AspNetCore.SignalR.Client;
namespace SignalR_NetClient;

class Program
{
    static void Main(string[] args)
    {
        bool connected = false;
        Console.WriteLine("Starting SignalR Client");
        string sHubUrl = "https://127.0.0.1:7088/chathub";
        HubConnection connection= new HubConnectionBuilder()
            //.WithUrl(new Uri("https://127.0.0.1:7088/chathub"))
            .WithUrl(sHubUrl, options => {
                options.UseDefaultCredentials = true;
                options.HttpMessageHandlerFactory = (msg) =>
                {
                    if (msg is HttpClientHandler clientHandler)
                    {
                        // bypass SSL certificate
                        clientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    }

                    return msg;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        
        connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0,5) * 1000);
            await connection.StartAsync();
        };
        //Recieving the "BroadCast" message
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"{user} : {message}");
        });
        //Reconnecting fucntion
        connection.Reconnecting += error =>
        {
            connected = false;
            Console.WriteLine($"Reconnecting:{connection.State == HubConnectionState.Reconnecting} with {error} ");
            return Task.CompletedTask;
        };
       //Start as last to register everything before it 
        connection.StartAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Console.WriteLine("There was an error opening the connection:{0}",
                    task.Exception.GetBaseException());
            } else {
                Console.WriteLine("Connected");
            }
        }).Wait();
        
        //While loop to keep the entire program running
        while (true)
        {
            if (connection.State == HubConnectionState.Connected && connected== false)
            {
                connected = true;
                Console.WriteLine("Connected in while loop");
                
            }
        }
        
    }// end of main

   
}