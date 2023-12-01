using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BIXBrokerApp_RabbitMQ
{
    class Program
    {

        static void Main(string[] args)
        { 
            Console.WriteLine("App is started ");
            MessageReceiver messageReceiver = new MessageReceiver();
            _ = messageReceiver.ProcessMessage();
            Console.WriteLine("Press Enter to exit...");
            
            //while (true)
            //{
            //    // Check for user input
            //    var userInput = Console.ReadLine();

            //    if (userInput == "q")
            //    {
            //        break; // Exit the loop if 'q' is entered
            //    }
            //}

            //Console.WriteLine("After Readline");
            //Console.WriteLine("After receiving 'q', exiting the application");
        }
    }
}
