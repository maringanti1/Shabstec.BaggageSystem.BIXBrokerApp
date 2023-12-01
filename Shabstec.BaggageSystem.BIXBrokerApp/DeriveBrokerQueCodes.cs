using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BIXBrokerApp_RabbitMQ
{
    public class DeriveBrokerQueCodes
    { 
        List<string> TopicCodes = new List<string>(); 

        public List<string> GetDeriveBrokerQueCodes(string NotifMessage)
        { 
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(NotifMessage);


            XmlNodeList xnList = xmlDoc.SelectNodes("/IATA_BagEventNotifRQ/Bag");

            foreach (XmlNode xn in xnList)
            {

                foreach (XmlNode xn2 in xn)
                {
                    if (xn2.OuterXml.Contains("BagSegment"))
                    {

                        foreach (XmlNode xn3 in xn2)
                        {

                            if (xn3.OuterXml.Contains("ArrivalStation"))
                            {

                                foreach (XmlNode xn4 in xn3)
                                {

                                    if (xn4.OuterXml.Contains("IATA_LocationCode"))
                                    {
                                        TopicCodes.Add(xn4.InnerText);


                                    }


                                }
                            }

                            if (xn3.OuterXml.Contains("DepStation"))
                            {

                                foreach (XmlNode xn4 in xn3)
                                {

                                    if (xn4.OuterXml.Contains("IATA_LocationCode"))
                                    {
                                        TopicCodes.Add(xn4.InnerText);


                                    }


                                }
                            }

                            if (xn3.OuterXml.Contains("OperatingCarrier"))
                            {

                                foreach (XmlNode xn4 in xn3)
                                {

                                    if (xn4.OuterXml.Contains("AirlineDesigCode"))
                                    {
                                        TopicCodes.Add(xn4.InnerText);


                                    }


                                }
                            }

                        }


                    }



                }



            }



            TopicCodes = TopicCodes.Distinct().ToList();
            return TopicCodes;
        }
    }
}
