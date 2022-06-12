using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Retry
    {
        public async static Task DoAsync(
            Action action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            await DoAsync<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public async static Task<T> DoAsync<T>(
            Func<Task<T>> action,
            TimeSpan retryInterval,
            int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
                await Task.Delay(retryInterval);
            }
            throw new AggregateException(exceptions);
        }
    }
}