namespace BIXBrokerApp_RabbitMQ
{
    public class BrokerMessage
    {
        public string id { get; set; }
        public string Message { get; set; }
        public string TopicHost { get; internal set; }
        public string TopicName { get; internal set; }
        public string ClientID { get; internal set; }
        public string ClientName { get; internal set; }

        public string ServiceID = "Message";
    }
}