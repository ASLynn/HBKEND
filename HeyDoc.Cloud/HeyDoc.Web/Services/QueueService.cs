using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Configuration;
using System.Text;

namespace HeyDoc.Web.Services
{
    public class QueueService
    {
        public static void PublishMessage(string queueName, object message)
        {
#if DEBUG
            //Staging VM machine was deleted
#else
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(ConfigurationManager.AppSettings["RabbitMQUri"]);
            var connection = factory.CreateConnection();
            var queueModel = connection.CreateModel();
            queueModel.QueueDeclarePassive(queueName);
            queueModel.BasicPublish("", queueName, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            connection.Close();
#endif


        }
    }
}