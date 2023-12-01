using System;
using System.Collections.Generic;
using System.Text;

namespace BIXBrokerApp_RabbitMQ.Model
{ 
        public class PODConfiguration
        {
            public string id { get; set; } // 'id' property is required
            public string ClientID { get; set; }
            public string ClientName { get; set; }

            #region Publisher Configuration 
            public string ServiceID { get; set; }
            public string AirLineCodes { get; set; }
            #endregion

            #region Message broker Configuration
            public string RabbitMQUsername { get; set; }
            public string RabbitMQPassword { get; set; }
            #endregion

            public string RabbitMQHost { get; set; }

            public int RabbitMQPort { get; set; }
        }  
}
