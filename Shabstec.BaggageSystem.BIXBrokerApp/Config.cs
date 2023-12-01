using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace BIXBrokerApp_RabbitMQ
{
    using System.Xml.Serialization;
    using System.Collections.Generic;

    [XmlRoot("Publisher")]
    public class Publisher
    {
        [XmlElement("ConfigData")]
        public ConfigData ConfigData { get; set; }

        [XmlElement("TopicCodeData")]
        public TopicCodeData TopicCodeData { get; set; }

        [XmlElement("SubscriberTopics")]
        public SubscriberTopics SubscriberTopics { get; set; }
    }

    public class ConfigData
    {
        [XmlElement("BrokerSvcBusQueueName")]
        public string BrokerSvcBusQueueName { get; set; }

        [XmlElement("BrokerSvcBusURL")]
        public string BrokerSvcBusURL { get; set; }

        [XmlElement("RabbitMQHostSecretName")]
        public string RabbitMQHostSecretName { get; set; }

        [XmlElement("RabbitMQPortSecretName")]
        public string RabbitMQPortSecretName { get; set; }

        [XmlElement("RabbitMQUsernameSecretName")]
        public string RabbitMQUsernameSecretName { get; set; }

        [XmlElement("RabbitMQPasswordSecretName")]
        public string RabbitMQPasswordSecretName { get; set; }

        [XmlElement("RabbitMQExchange")]
        public string RabbitMQExchange { get; set; }
        [XmlElement("SubscribedQueueName")]
        public string SubscribedQueueName { get; set; }
        [XmlElement("SubscribedQueueEndPoint")]
        public string SubscribedQueueEndPoint { get; set; }
    }

    public class TopicCodeData
    {
        [XmlArray("Topics")]
        [XmlArrayItem("Topic")]
        public List<Topic> Topics { get; set; }
    }

    public class SubscriberTopics
    {
        [XmlArray("SubTopics")]
        [XmlArrayItem("Topic")]
        public List<Topic> SubTopics { get; set; }
    }

    public class Topic
    {
        [XmlElement("TopicName")]
        public string TopicName { get; set; }

        [XmlElement("TopicHost")]
        public string TopicHost { get; set; }

        [XmlElement("TopicVersion")]
        public string TopicVersion { get; set; }
    }

}