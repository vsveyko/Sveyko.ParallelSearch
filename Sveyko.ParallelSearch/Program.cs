using System;

namespace Sveyko.ParallelSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Searcher searcher = new Searcher();
            searcher.DegreeOfParallelism = 2;
            searcher.GenerateBaseData();
            string line = "start";
            while (line != "exit")
            {
                Console.Clear();
                Console.WriteLine("ParallelSearch Demo");
                Console.WriteLine("Degree of parallelism: {0}", searcher.DegreeOfParallelism);
                Console.WriteLine("Enter search value:");
                line = Console.ReadLine();
                searcher.SearchLine = line;
                if (searcher.FilterResults())
                    Console.WriteLine("Search results are valid");
                else
                    Console.WriteLine("ERROR: Search sesults are invalid!");
                Console.WriteLine("Press any key to new search or type 'exit' to quit from the program...");
                line = Console.ReadLine();
            }
        }
    }
}
