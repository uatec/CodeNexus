using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookSleeve;
using CodeNexus;

namespace CodeRaven.Data.Redis
{
    public class RedisIndex : IIndex
    {
        private readonly int _indexDb;
        private readonly RedisConnection _connection;
        private volatile Task _openingTask;
        private readonly object _syncRoot = new object();
        public RedisIndex(string hostname, int port, int indexDb)
        {
            _indexDb = indexDb;
            
            _connection = new RedisConnection(hostname, port);
        }

        public IEnumerable<string> Get(string value)
        {
            return this.GetAsync(value).Result;
        }

        public Task<IEnumerable<string>> GetAsync(string value)
        {
            if (_connection.State == RedisConnectionBase.ConnectionState.New)
            {
                if (_openingTask == null)
                {
                    lock (_syncRoot)
                    {
                        if (_openingTask == null)
                        {
                            _openingTask = _connection.Open();
                        }
                    }
                }
                _openingTask.Wait();
            }
            return Task<IEnumerable<string>>.Factory.StartNew(() => _connection.Sets.GetAllString(_indexDb, value).Result.AsEnumerable());
        }

        public void Add(string key, string value)
        {
            this.AddAsync(key, value).Wait();
        }

        public Task AddAsync(string key, string value)
        {
            if (_connection.State == RedisConnectionBase.ConnectionState.New)
            {
                if (_openingTask == null)
                {
                    lock (_syncRoot)
                    {
                        if (_openingTask == null)
                        {
                            _openingTask = _connection.Open();
                        }
                    }
                }
                _openingTask.Wait();
            }
            return _connection.Sets.Add(_indexDb, value, key);
        }
    }
}
