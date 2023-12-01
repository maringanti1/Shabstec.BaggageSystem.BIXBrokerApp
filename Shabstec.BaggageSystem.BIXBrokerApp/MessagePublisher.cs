using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using BlazorApp.API.Models;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using BIXBrokerApp_RabbitMQ.Model;
using System.Security.Policy;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace BIXBrokerApp_RabbitMQ
{
    public class MessagePublisher
    {
        private readonly CosmosDbSettings _cosmosDbSettings;

        public MessagePublisher()
        {
            _cosmosDbSettings = new CosmosDbSettings();
            _cosmosDbSettings.Endpoint = System.Configuration.ConfigurationManager.AppSettings["Endpoint"];
            _cosmosDbSettings.Key = System.Configuration.ConfigurationManager.AppSettings["Key"];
            _cosmosDbSettings.DatabaseName = System.Configuration.ConfigurationManager.AppSettings["DatabaseName"];
            _cosmosDbSettings.ContainerName = System.Configuration.ConfigurationManager.AppSettings["ContainerName"];
            _cosmosDbSettings.Environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
        }

        public void SendMessage(string message, string brokerUrl, string topicName, 
            Publisher publisher)
        {
            try
            {
                // Generate or load the client certificate
                //string subjectName = "Client1";
                //X509Certificate2 clientCertificate = CertificateManager.GenerateSelfSignedCertificate(subjectName);
                
                if (publisher != null)
                {
                    Console.WriteLine(publisher.ConfigData.RabbitMQHostSecretName);
                    Console.WriteLine(publisher.ConfigData.RabbitMQPortSecretName);
                    Console.WriteLine(publisher.ConfigData.RabbitMQUsernameSecretName);
                    Console.WriteLine(publisher.ConfigData.RabbitMQPasswordSecretName);
                    string rabbitMQHost = publisher.ConfigData.RabbitMQHostSecretName;
                    int rabbitMQPort = Convert.ToInt32(publisher.ConfigData.RabbitMQPortSecretName);
                    string rabbitMQUsername = publisher.ConfigData.RabbitMQUsernameSecretName;
                    string rabbitMQPassword = publisher.ConfigData.RabbitMQPasswordSecretName;
                    RabbitMQManager rabbitMQManager = new RabbitMQManager();
                    // Initialize RabbitMQ connection and channel 
                    string result =  rabbitMQManager.InitializeRabbitMQ(rabbitMQHost, rabbitMQPort, rabbitMQUsername, 
                        rabbitMQPassword, _cosmosDbSettings.Environment);

                    if (string.IsNullOrEmpty(result))
                    {
                        Console.WriteLine("Initialize RabbitMQ is success: " + rabbitMQHost);

                        rabbitMQManager.PublishMessageToRabbitMQ(message, 
                                                            brokerUrl, 
                                                            topicName,
                                                            _cosmosDbSettings, 
                                                            publisher.ConfigData.RabbitMQExchange,
                                                            rabbitMQHost);
                        rabbitMQManager.Close();
                    }
                } 
                // Publish the message to RabbitMQ using dynamic brokerUrl and topicName 
                // Check others client who is registed for this broker
                // Check in Cosmos DB for all the Clients who is resitered with this broker.
                InvokeInterCommunicator(message, brokerUrl, topicName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private void InvokeInterCommunicator(string message, string brokerUrl, string topicName)
        {
            try
            {
                Console.WriteLine("InvokeInterCommunicator is called with " + brokerUrl + "_" + topicName);
                // Publish the message to RabbitMQ using dynamic brokerUrl and topicName 
                // Check others client who is registed for this broker
                // Check in Cosmos DB for all the Clients who is resitered with this broker.
                var clientConfigurations = GetAllConfigurations();
                if (clientConfigurations != null)
                {
                    Console.WriteLine("clientConfigurations count: " + clientConfigurations.Result.Count);
                    foreach (PODConfiguration clientConfiguration in clientConfigurations.Result)
                    {
                        // Split the comma-separated string into an array of airline codes
                        string[] airlineCodesArray = clientConfiguration.AirLineCodes.Split(',');
                        // Check if the array contains the specified topicName
                        if (airlineCodesArray.Contains(topicName)) 
                            {
                            Console.WriteLine("Airline code is subscribed for : " + clientConfiguration.ClientName);

                            RabbitMQManager rabbitMQManager = new RabbitMQManager();
                            Console.WriteLine("Initialize RabbitMQ: " + clientConfiguration.RabbitMQHost);
                            string result = rabbitMQManager.InitializeRabbitMQ(clientConfiguration.RabbitMQHost,
                            5672, clientConfiguration.RabbitMQUsername,
                                 clientConfiguration.RabbitMQPassword, _cosmosDbSettings.Environment);
                            if (string.IsNullOrEmpty(result))
                            {
                                Console.WriteLine("Initialize RabbitMQ is success: " + clientConfiguration.RabbitMQHost);
                                rabbitMQManager.PublishMessageToRabbitMQ(message, 
                                    clientConfiguration.RabbitMQHost, 
                                    topicName,
                                    _cosmosDbSettings,
                                    clientConfiguration.ClientID,
                                    clientConfiguration.RabbitMQHost);

                            }
                            else
                            {
                                Console.WriteLine("Initialize RabbitMQ is failed at: " + clientConfiguration.RabbitMQHost);
                                Console.WriteLine("Initialize RabbitMQ is failed. " + result);
                            }
                            rabbitMQManager.Close();
                        }
                        else
                        {
                            Console.WriteLine($"{topicName} is not in the list of airline codes.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at: InvokeInterCommunicator " + ex.Message);
                throw ex;
            }
        }





        public async Task<List<PODConfiguration>> GetAllConfigurations()
        {
            var cosmosClient = new CosmosClient(_cosmosDbSettings.Endpoint, _cosmosDbSettings.Key);
            var database = cosmosClient.GetDatabase(_cosmosDbSettings.DatabaseName);
            var container = database.GetContainer(_cosmosDbSettings.ContainerName);
            try
            {
                using (cosmosClient)
                {

                    var query = new QueryDefinition("SELECT * FROM c where c.ServiceID='PODConfiguration'");
                    var iterator = container.GetItemQueryIterator<PODConfiguration>(query);

                    var configurations = new List<PODConfiguration>();
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        configurations.AddRange(response.ToList());
                    }
                    return configurations;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Handle any exceptions and return an error response
                return null;
            }
        }
    }
}
