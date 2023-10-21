﻿using System;
using System.Collections;
using System.Collections.Generic;
using KestrelMockServer.Settings;

namespace KestrelMockServer.Domain
{
    public class Watcher
    {
        private readonly Dictionary<Guid, Queue<WatchLog>> watchLogs = new Dictionary<Guid, Queue<WatchLog>>();

        public void Log(string path, string body, string method, Watch watch)
        {
            var watchLog = new WatchLog(path, body, method);

            if (!watchLogs.ContainsKey(watch.Id))
            {
                watchLogs.Add(watch.Id, new Queue<WatchLog>());
            }

            watchLogs[watch.Id].Enqueue(watchLog);
            if (watchLogs[watch.Id].Count > watch.NumberOfRequests)
            {
                watchLogs[watch.Id].Dequeue();
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
                this.watchLogs.Remove(watchId);
            }
        }
    }
}
