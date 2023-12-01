using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.IO;
using System.Xml.Serialization;
using BlazorApp.API.Models;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace BIXBrokerApp_RabbitMQ
{

    public class RabbitMQManager
    {
        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;
        public RabbitMQManager()
        {
            
            
        }

        public string InitializeRabbitMQ(string host, int port, string username, string password, string environment)
        {
            string initializeStatus = "";
            try
            {
                Console.WriteLine("InitializeRabbitMQ: host " + host + " port " + port + 
                    " username "+ username + " password " + password + "environment " + environment);
                if (environment.ToLower() != "dev")
                {
                    factory = new ConnectionFactory
                    {
                        HostName = host,
                        Port = port,
                        UserName = username,
                        Password = password,
                        VirtualHost = environment
                    };
                }
                else
                {
                    factory = new ConnectionFactory
                    {
                        HostName = host,
                        Port = port,
                        UserName = username,
                        Password = password
                    };
                }
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                Console.WriteLine($"RabbitMQ channel established");
                return initializeStatus;

            }
            catch (Exception ex)
            {
                initializeStatus = ex.Message;                
                Console.WriteLine($"An error occurred while initializing RabbitMQ: {ex.Message}");
                return initializeStatus;
            }
        }

        public void DeclareQueue(string queueName)
        {
             // Declare a queue
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void DeclareExchange(string exchangeName, string exchangeType)
        {
            // Declare an exchange
            channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);
        }

        public void BindQueueToExchange(string queueName, string exchangeName, string routingKey)
        {
            // Bind the queue to the exchange with a routing key
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
        }
        public async void PublishMessageToRabbitMQ( string message, string exchangeName, string routingKey,
           CosmosDbSettings _cosmosDbSettings, string ClientID, string  host)
        {
            try
            {
                Console.WriteLine($"Sending message to Rabbitmq started");
                // Declare a queue
                DeclareQueue(routingKey);
                // Declare the exchange
                DeclareExchange(exchangeName, ExchangeType.Direct);
                // Bind the queue to the exchange with a routing key
                BindQueueToExchange(routingKey, exchangeName, routingKey);

                // Publish the message to the exchange with the specified routing key (topicName)
                byte[] body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchangeName, routingKey, null, body);
                Console.WriteLine($"Sent message to RabbitMQ: {message}");
                await SaveMessageToDB(message, host, routingKey,
                _cosmosDbSettings, ClientID, routingKey);
                Console.WriteLine($"SaveMessageToDB is completed! "+ exchangeName +"_"+ routingKey
                    + "_" + ClientID + "_" + routingKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while publishing messages to RabbitMQ: {ex.Message}");
                throw ex;
            }
        }

        
        public void Close()
        {
            // Close the channel and connection
            channel.Close();
            connection.Close();
        }

        public async Task SaveMessageToDB(string message, string host, string routingKey,
            CosmosDbSettings _cosmosDbSettings, string ClientID, string topicName)
        {
            var cosmosClient = new CosmosClient(_cosmosDbSettings.Endpoint, _cosmosDbSettings.Key);
            var database = cosmosClient.GetDatabase(_cosmosDbSettings.DatabaseName);
            var container = database.GetContainer(_cosmosDbSettings.ContainerName);
            try
            {
                BrokerMessage brokerMessage = new BrokerMessage();
                brokerMessage.id = System.Guid.NewGuid().ToString();
                brokerMessage.Message = message;
                brokerMessage.TopicHost = host;
                brokerMessage.ClientID = ClientID;
                brokerMessage.ClientName = ClientID;
                brokerMessage.TopicName = topicName;
                brokerMessage.ServiceID = "Message"; 
                var response = await container.CreateItemAsync(brokerMessage, new PartitionKey(brokerMessage.ServiceID));

                //_logger.LogInformation($"Broker message with id: {response.Resource.id}");
                //EmailManager emailManager = new EmailManager(_emailConfig);
                //emailManager.SendEmail(user);
                //Send an email
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
                
            }
        }
    }
}