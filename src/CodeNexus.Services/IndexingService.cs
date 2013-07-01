using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CodeNexus.Services
{
    public class IndexingService : BasicService
    {
        private readonly IIndex _index;

        public IndexingService(IIndex index)
        {
            _index = index;
        }

        private int _filesIndexed = 0;
        public int FilesIndexed { get { return _filesIndexed; } }

        private IEnumerable<T> SafeEnumerate<T>(IEnumerable<T> enumerable)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            bool endOfList = false;
            while (!endOfList)
            {
                try
                {
                    endOfList = !enumerator.MoveNext();
                }
                catch (Exception)
                {
                    // shut up, let's skip this one
                    continue;
                }
                yield return enumerator.Current;
            }
        }


        protected override void _serviceTask(CancellationToken cancellationToken)
        {
            Thread thread = Thread.CurrentThread;
            cancellationToken.Register(thread.Abort);

            DirectoryInfo di = new DirectoryInfo("C:\\Development\\");
            Regex wordFinder = new Regex("[A-Z0-9]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            foreach (FileInfo fileInfo in SafeEnumerate(di.EnumerateFiles("*.cs", SearchOption.AllDirectories)))
            {
                try
                {

                    using (FileStream fileStream = fileInfo.OpenRead())
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                string line = streamReader.ReadLine().ToLower();

                                MatchCollection matches = wordFinder.Matches(line);

                                foreach (Match match in matches)
                                {
                                    _index.Add(fileInfo.FullName, match.Value);
                                }
                            }
                        }
                    }
                }
                catch (AggregateException aggregateException)
                {
                    foreach ( Exception ex in aggregateException.InnerExceptions)
                    {
                        Console.WriteLine("{0} ! {1}", fileInfo.FullName, ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0} ! {1}", fileInfo.FullName, ex.Message);
                }
                _filesIndexed++;
            }
        }
    }
}