using HeyDoc.Web.Helpers;
using HeyDoc.Web.Models;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.Services
{
    public class ServiceBusService
    {
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings["ServiceBusConnection"].ConnectionString;

        public async static Task ScheduleQueueMessage(string serviceBusQueueName, string message, DateTimeOffset scheduledDateTime)
        {
            if (scheduledDateTime.Subtract(DateTimeOffset.UtcNow).TotalDays > 13)
            {
                throw new Exception("Only possible to schedule message a maximum of 13 days away.");
            }

            var queueClient = await _CreateQueueClientInstance(serviceBusQueueName);
            var brokeredMessage = new BrokeredMessage(message)
            {
                // Azure Service Bus messages only live to a maximum of 14 days on Basic plan
                ScheduledEnqueueTimeUtc = scheduledDateTime.UtcDateTime
            };
            await queueClient.SendAsync(brokeredMessage);
        }

        public async static Task<ServiceBusQueueMessage<T>> PeekSerializedQueueMessage<T>(string serviceBusQueueName, long sequenceNumber)
        {
            var queueClient = await _CreateQueueClientInstance(serviceBusQueueName);

            var message = await queueClient.PeekAsync(sequenceNumber);
            return new ServiceBusQueueMessage<T>
            {
                SequenceNumber = sequenceNumber,
                Message = JsonConvert.DeserializeObject<T>(message.GetBody<string>())
            };
        }

        public async static Task<List<ServiceBusQueueMessage<T>>> PeekSerializedQueueMessages<T>(string serviceBusQueueName, int messageCount, long? fromSequenceNumber = null)
        {
            var queueClient = await _CreateQueueClientInstance(serviceBusQueueName);

            List<BrokeredMessage> messages;
            if (fromSequenceNumber.HasValue)
            {
                messages = (await queueClient.PeekBatchAsync(fromSequenceNumber.Value, messageCount)).ToList();
            }
            else
            {
                messages = (await queueClient.PeekBatchAsync(messageCount)).ToList();
            }

            var enqueuedMessages = messages.Select(m => new ServiceBusQueueMessage<T>
            {
                SequenceNumber = m.SequenceNumber,
                Message =  JsonConvert.DeserializeObject<T>(m.GetBody<string>())
            }).ToList();
            return enqueuedMessages;
        }

        public async static Task DeleteFromQueue(string serviceBusQueueName, long sequenceNumber)
        {
            var queueClient = await _CreateQueueClientInstance(serviceBusQueueName);
            await queueClient.CancelScheduledMessageAsync(sequenceNumber);
        }

        private async static Task<QueueClient> _CreateQueueClientInstance(string serviceBusQueueName)
        {
            var namespaceMgr = NamespaceManager.CreateFromConnectionString(_connectionString);
            try
            {
                await namespaceMgr.CreateQueueAsync(serviceBusQueueName);
            }
            catch (MessagingEntityAlreadyExistsException) { }

            return QueueClient.CreateFromConnectionString(_connectionString, serviceBusQueueName);
        }
    }
}