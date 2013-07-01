using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Embedded;
using System.Threading;

namespace CodeNexus.Data.RavenDB
{
    public class Class1
    {
        public class Person
        {
            public DateTime Date { get; set; }
            public string Name { get; set; }
        }

        public Class1()
        {
            Person myObject = new Person()
                                   {
                                       Date = DateTime.Now,
                                       Name = "Jack"
                                   };

            var documentStore = new EmbeddableDocumentStore()
                                    {
                                        DataDirectory = "Data"
                                    };
            documentStore.Initialize();
            Console.WriteLine("inited");
            var session = documentStore.OpenSession();
            Console.WriteLine("session open");
            session.Store(myObject);
            session.SaveChanges();
            Console.WriteLine("changes saved");
            Thread.Sleep(1000);
            foreach (Person queryResponse in session.Query<Person>().Where(o => o.Name == "Jack"))
            {
                Console.WriteLine(queryResponse.Name + ".");
            }
            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}
