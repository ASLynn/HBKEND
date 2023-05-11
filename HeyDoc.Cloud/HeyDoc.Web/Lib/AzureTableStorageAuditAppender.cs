using log4net.Appender;
using log4net.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// Has dependency on ElasticTableEntity from log4net.Appender.Azure

namespace HeyDoc.Web.Lib
{
    // Based on AzureTableStorageAppender from log4net.Appender.Azure package
    public class AzureTableStorageAuditAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;

        public string ConnectionStringName { get; set; }

        private string _connectionString;
        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ConnectionStringName))
                {
                    return Util.GetConnectionString(ConnectionStringName);
                }
                if (string.IsNullOrEmpty(_connectionString))
                    throw new ApplicationException("No Azure Storage connection string specified");
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }


        private string _tableName;
        public string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                    throw new ApplicationException("No table name specified");
                return _tableName;
            }
            set
            {
                _tableName = value;
            }
        }

        public PartitionKeyTypeEnum PartitionKeyType { get; set; } = PartitionKeyTypeEnum.LoggerName;

        private string _partitionKeyProperty;
        public string PartitionKeyProperty
        {
            get
            {
                if (string.IsNullOrEmpty(_partitionKeyProperty))
                    throw new ApplicationException("No property for partition key specified");
                return _partitionKeyProperty;
            }
            set
            {
                _partitionKeyProperty = value;
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            // Group by PartitionKey so they can be sent in separate batches as a batch must only operate on a single PartitionKey
            var partitionGroups = events.Select(GetLogEntity).GroupBy(e => e.PartitionKey);

            foreach (var group in partitionGroups)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var azureLoggingEvent in group)
                {
                    batchOperation.Insert(azureLoggingEvent);
                }
                _table.ExecuteBatch(batchOperation);
            }
        }

        private ITableEntity GetLogEntity(LoggingEvent @event)
        {
            var tableEntity = new ElasticTableEntity
            {
                RowKey = $"{DateTime.MaxValue.Ticks - @event.TimeStampUtc.Ticks:D19}.{Guid.NewGuid().ToString().ToLower()}",
            };
            switch (PartitionKeyType)
            {
                case PartitionKeyTypeEnum.LoggerName:
                    tableEntity.PartitionKey = @event.LoggerName;
                    break;
                case PartitionKeyTypeEnum.DateHour:
                    tableEntity.PartitionKey = @event.TimeStampUtc.Date.AddHours(@event.TimeStampUtc.Hour).ToString("yyyy-MM-ddTHH:mm");
                    break;
                case PartitionKeyTypeEnum.Property:
                    tableEntity.PartitionKey = Regex.Replace(@event.Properties[PartitionKeyProperty] as string, @"[/\\#\?]", "_");
                    break;
            }
            tableEntity["EventTimestamp"] = @event.TimeStampUtc;
            tableEntity["Level"] = @event.Level.ToString();
            tableEntity["Message"] = $"{@event.RenderedMessage}\n{@event.GetExceptionString()}";
            foreach (var prop in @event.Properties.GetKeys())
            {
                if (prop.StartsWith("Audit_"))
                {
                    tableEntity[prop.Substring(6)] = @event.Properties[prop] as string;
                }
            }
            if (Layout != null)
            {
                using (var writer = new StringWriter())
                {
                    Layout.Format(writer, @event);
                    tableEntity["Message"] = writer.ToString();
                }
            }

            return tableEntity;
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(TableName);
            //_table.CreateIfNotExists();
        }
    }

    public enum PartitionKeyTypeEnum
    {
        /// <summary>
        /// Each logger gets its own partition in Table Storage
        /// </summary>
        LoggerName,
        /// <summary>
        /// Use timestamp rounded down to hour as PartitionKey
        /// </summary>
        DateHour,
        /// <summary>
        /// Use the custom log4net property named in PartitionKeyProperty
        /// </summary>
        Property
    }
}