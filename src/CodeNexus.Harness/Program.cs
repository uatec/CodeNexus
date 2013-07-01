using System;
using CodeNexus.Services;
using CodeRaven.Data.Redis;

namespace CodeNexus.Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            IIndex index = new RedisIndex("localhost", 6379, 0);
            IService service = new IndexingService(index);
            service.Start();

            while (true)
            {
                Console.WriteLine("{0} files indexed.", (service as IndexingService).FilesIndexed);
                Console.Write("> ");
                string term = Console.ReadLine().ToLower();

                if (String.IsNullOrEmpty(term))
                {
                    continue;
                }

                int resultsFound = 0;
                foreach (string file in index.Get(term))
                {
                    Console.ForegroundColor = resultsFound%2 == 0 ? ConsoleColor.Gray : ConsoleColor.DarkGray;
                    Console.WriteLine("{0}: {1}", resultsFound++, file);
                }
                Console.ResetColor();
            }
        }
    }
}
