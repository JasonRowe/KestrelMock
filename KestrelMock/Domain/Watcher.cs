using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Domain
{
    public class Watcher
    {
        private readonly ConcurrentDictionary<Guid, Queue<WatchLog>> watchLogs = new ConcurrentDictionary<Guid, Queue<WatchLog>>();
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Services.TrafficHub> _hubContext;

        public Watcher(Microsoft.AspNetCore.SignalR.IHubContext<Services.TrafficHub> hubContext = null)
        {
            _hubContext = hubContext;
        }

        public void Log(string path, string body, string method, Watch watch)
        {
            var watchLog = new WatchLog(path, body, method);

            if (!watchLogs.ContainsKey(watch.Id))
            {
                watchLogs.TryAdd(watch.Id, new Queue<WatchLog>());
            }

            watchLogs[watch.Id].Enqueue(watchLog);
            if (watchLogs[watch.Id].Count > watch.RequestLogLimit)
            {
                watchLogs[watch.Id].Dequeue();
            }

            if (_hubContext != null)
            {
                _ = _hubContext.Clients.All.SendAsync("ReceiveTraffic", watchLog, watch.Id);
            }
        }

        public WatchLog[] GetWatchLogs(Guid watchId)
        {
            return watchLogs.ContainsKey(watchId)
                ? DequeueAll(watchLogs[watchId])
                : Array.Empty<WatchLog>();
        }

        private WatchLog[] DequeueAll(Queue<WatchLog> queue)
        {
            var list = new List<WatchLog>();

            while (queue.Count > 0)
            {
                list.Add(queue.Dequeue());
            }

            return list.ToArray();
        }

        public void Remove(Guid watchId)
        {
            if (watchLogs.ContainsKey(watchId))
            {
                this.watchLogs.TryRemove(watchId, out var mockFound);
            }
        }
    }
}
