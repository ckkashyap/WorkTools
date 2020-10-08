using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusListener
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            var subscription1Client = new SubscriptionClient(connectionString, "bot", "kaizala");

            InitializeReceiver(subscription1Client, ConsoleColor.White);
            Console.WriteLine("Hello World!");


            await Task.WhenAny(
               Task.Run(() => Console.ReadKey()),
               Task.Delay(TimeSpan.FromSeconds(60*60*5))
           );
        }

        static void InitializeReceiver(SubscriptionClient receiver, ConsoleColor color)
        {
            // register the RegisterMessageHandler callback
            receiver.RegisterMessageHandler(
                async (message, cancellationToken) =>
                {
                    if (true)
                    {
                        var body = message.Body;
                        var str = Encoding.UTF8.GetString(body);

                        //dynamic json = JsonConvert.DeserializeObject(str);

                        using (var fs = File.AppendText(@"c:\s\messages.txt"))
                        {
                            fs.WriteLine(str);

                        }

                            lock (Console.Out)
                            {
                                Console.ForegroundColor = color;
                                Console.WriteLine(str);
                                Console.ResetColor();
                            }
                    }
                    await receiver.CompleteAsync(message.SystemProperties.LockToken);
                },
                new MessageHandlerOptions( (e)=>LogMessageHandlerException(e) ) { AutoComplete = false, MaxConcurrentCalls = 1 });
        }

        static private Task LogMessageHandlerException(ExceptionReceivedEventArgs e)
        {
            Console.WriteLine("Exception: \"{0}\" {1}", e.Exception.Message, e.ExceptionReceivedContext.EntityPath);
            return Task.CompletedTask;
        }
    }
}
