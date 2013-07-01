using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeNexus
{
    public class MemoryIndex : IIndex
    {
        private ConcurrentDictionary<string, ConcurrentBag<string>> _data = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        public Task<IEnumerable<string>> GetAsync(string value)
        {
            return Task<IEnumerable<string>>.Factory.StartNew(() => this.Get(value));
        }

        public IEnumerable<string> Get(string value)
        {
            if (_data.ContainsKey(value))
            {
                foreach (string s in _data[value])
                {
                    yield return s;
                }
            }
        }

        public Task AddAsync(string key, string value)
        {
            return Task.Factory.StartNew(() => this.Add(key, value));
        }

        public void Add(string key, string value)
        {
            _data.GetOrAdd(value, (v) => new ConcurrentBag<string>()).Add(key);
        }
    }
}