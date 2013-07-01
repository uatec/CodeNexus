using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeNexus
{
    public interface IIndex
    {
        IEnumerable<string> Get(string value);
        void Add(string key, string value);

        Task<IEnumerable<string>> GetAsync(string value);
        Task AddAsync(string key, string value);
    }
}