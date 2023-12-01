using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BIXBrokerApp_RabbitMQ;
using BIXBrokerApp_RabbitMQ.Model;
using BlazorApp.API.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Azure.Core.HttpHeader;

namespace BIXBrokerApp_RabbitMQ
{
    public class MessageReceiver 
    {
        private static Publisher publisher;
        public async Task ProcessMessage()
        {
            string xmlData = "";
            //IConfiguration configuration = new ConfigurationBuilder()
            //    .AddJsonFile("broker_config.json", optional: true, reloadOnChange: true)
            //    .Build();
            // Get the XML data from the environment variable
            string xmlDataFromEnvironment = Environment.GetEnvironmentVariable("XMLFILE");
            if (string.IsNullOrEmpty(xmlDataFromEnvironment))
            {
                Console.WriteLine("XMLFILE environment variable not found");
                Console.WriteLine("ProcessMessage is started");
                Console.WriteLine("Checking for PublisherConfiguration xml file in local folder");
                // Check if the file exists in the directory of the executable
                string executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePathInExecutableDir = Path.Combine(executableDirectory, "PublisherConfiguration.xml");

                if (!File.Exists(filePathInExecutableDir))
                {
                    Console.WriteLine("PublisherConfiguration.xml file is not found in " + filePathInExecutableDir);
                    return;
                }
                else
                {
                    Console.WriteLine("PublisherConfiguration.xml file is found in " + filePathInExecutableDir);
                }
                // Load XML from a file or another source
                 xmlData = File.ReadAllText("PublisherConfiguration.xml");
            }
            else
            {
                // Convert the Base64 string to bytes
                byte[] base64Bytes = Convert.FromBase64String(xmlDataFromEnvironment);

                // Convert the bytes to an XML string
                xmlData = Encoding.UTF8.GetString(base64Bytes);
            }

            try
            {
                Console.WriteLine(xmlData);
                // Create an XmlSerializer for ConfigData
                XmlSerializer serializer = new XmlSerializer(typeof(Publisher));

                using (StringReader reader = new StringReader(xmlData))
                {
                     publisher = (Publisher)serializer.Deserialize(reader);
                    // Now, you can access the configuration data using publisher object.
                }

                //keyVaultConfig = configuration.GetSection("ConfigData").Get<ConfigData>();
                //topicCodeData = configuration.GetSection("TopicCodeData").Get<TopicCodeData>();

                //string KeyVaultUrl = keyVaultConfig.KeyVaultUrl;
                string BrokerSvcBusURL = publisher.ConfigData.BrokerSvcBusURL;
                string BrokerSvcBusQueueName = publisher.ConfigData.BrokerSvcBusQueueName;
                Console.WriteLine(BrokerSvcBusURL);
                Console.WriteLine(BrokerSvcBusQueueName);
                // Replace "ConnectionString" with your actual Service Bus connection string
                var queueClient = new QueueClient(BrokerSvcBusURL, BrokerSvcBusQueueName);
                Console.WriteLine("Client estabished");
                
                // Register the message handler to receive messages
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false,
                    MaxAutoRenewDuration = TimeSpan.FromMinutes(5) // Set this to a reasonable duration
                };
                Console.WriteLine("Waiting for new messages");
                // Define the message handler
                queueClient.RegisterMessageHandler(async (receivedMessage, cancellationToken) =>
                {
                    try
                    {
                        Console.WriteLine("Retrieving new message from " + BrokerSvcBusQueueName);
                        // Extract the XML and publish codes from the received message
                        string xmlMessage = Encoding.UTF8.GetString(receivedMessage.Body);
                       
                        // Get codes from the list
                        DeriveBrokerQueCodes deriveBrokerQueCodes = new DeriveBrokerQueCodes();
                        List<string> deriveBrokerQueList = deriveBrokerQueCodes.GetDeriveBrokerQueCodes(xmlMessage);
                        string joinedString = string.Join(",", deriveBrokerQueList); // Use a comma as the separator
                        Console.WriteLine(joinedString); 
                        Console.WriteLine("Derived Codes from the message are " + joinedString);
                        foreach (var item in deriveBrokerQueList)
                        {
                            foreach (var code in publisher.TopicCodeData.Topics)
                            {
                                if (code.TopicName.ToLower() == item.ToLower())
                                {
                                    Console.WriteLine("Derived Code from the message " + code.TopicName +" is in Clients configuration list");
                                    // Publish the message to all matched URLs
                                    PublishMessage(xmlMessage, code.TopicName, code.TopicHost, publisher);
                                    Console.WriteLine("Publish Message of " + code.TopicName + " is completed");


                                }
                            }
                        }
                        Console.WriteLine("Waiting for new messages");
                        await queueClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        // Dead-letter the message or handle the error as needed
                        await queueClient.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                    }
                }, messageHandlerOptions);

                // Keep the application running to receive messages
                //Console.WriteLine("Press any key to exit...");
                //Console.ReadKey();
                Console.WriteLine("While loop started...");
                while (true)
                {
                    // Your message processing logic here

                    // Sleep for a while to avoid busy-waiting
                    Thread.Sleep(millisecondsTimeout: 10000); // Sleep for 1 second (adjust as needed)
                }
                // Close the queue client
                await queueClient.CloseAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                throw ex;
            }
            }


        //static string GetSecretValue(string keyVaultUrl, string secretName)
        //{
        //    try
        //    {
        //        //// Create an instance of the SecretClient class
        //        //var client = new SecretClient(new Uri(keyVaultUrl), 
        //        //    new DefaultAzureCredential());

        //        // Replace with your client ID, client secret, and tenant ID
        //        string clientId = "f1614a6d-4554-453e-8c0a-9c567b1ead66";
        //        string clientSecret = "dw-8Q~ceABclqjqJAAsm2w4rN9RF0xAl9Rf-Ady9";
        //        //a5e69239-b1bf-437a-b95e-c88af1378d3f
        //        string tenantId = "432923c4-2803-45f9-9599-12fca81ec374";
        //        // Create an instance of the ClientSecretCredential
        //        var clientCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        //        // Create an instance of the SecretClient class using the ClientSecretCredential
        //        var client = new SecretClient(new Uri(keyVaultUrl), clientCredential);

        //        // Retrieve the secret using its name
        //        var secret = client.GetSecret(secretName);
        //        // Retrieve the secret using its name
        //        //var secret = client.GetSecret(secretName);

        //        // Return the secret value
        //        return secret.Value.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred: {ex.Message}");
        //        return null;
        //    }
        //}




        static void PublishMessage(string xml, string url, string code, Publisher publisher)
        {
            // send message to cosmos db

            MessagePublisher messagePublisher = new MessagePublisher();
            messagePublisher.SendMessage(xml, url, code, publisher);

            Console.WriteLine($"Message published to {url} {code}");
        }

        // Define the exception handler for message handling errors
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs args)
        {
            // Handle the exception
            Console.WriteLine($"Message handler encountered an exception: {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}
