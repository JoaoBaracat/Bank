using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Bank.Api.Consumers
{
    public class RetryHandler
    {
        private const string RetryHeader = "RETRY-COUNT";

        public IDictionary<string, object> CopyMessageHeaders(IDictionary<string, object> existingProperties)
        {
            var newProperties = new Dictionary<string, object>();
            if (existingProperties != null)
            {
                var enumerator = existingProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    newProperties.Add(enumerator.Current.Key, enumerator.Current.Value);
                }
            }
            return newProperties;
        }

        public void SetRetryAttempts(IBasicProperties properties, int newAttempts)
        {
            if (properties.Headers.ContainsKey(RetryHeader))
                properties.Headers[RetryHeader] = newAttempts;
            else
                properties.Headers.Add(RetryHeader, newAttempts);
        }

        public int GetRetryAttempts(IBasicProperties properties)
        {
            if (properties.Headers == null || properties.Headers.ContainsKey(RetryHeader) == false)
                return 0;

            var val = properties.Headers[RetryHeader];
            if (val == null)
                return 0;

            return Convert.ToInt32(val);
        }
    }
}
