using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sveyko.ParallelSearch
{
    class Searcher
    {
        private IEnumerable<string> BaseData;
        private IEnumerable<string> FilteredData;
        public string SearchLine {get; set;}
        private int degreeOfParallelism;
        public int DegreeOfParallelism {
            get => degreeOfParallelism;
            set {
                    degreeOfParallelism = value;
                    if (degreeOfParallelism < 1)
                        degreeOfParallelism = 1;
                    if (degreeOfParallelism > 5)
                        degreeOfParallelism = 5;                
                }
        }

        public void GenerateBaseData()
        {
            char[] alphaArr = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i).ToArray();
            var collections = new [] { alphaArr, alphaArr, alphaArr, alphaArr };
            var cartesianCollection = External.Cartesian(collections);
            BaseData = (from item in cartesianCollection.AsParallel().WithDegreeOfParallelism(DegreeOfParallelism)
                        select string.Concat(item)
                       );
        }

        public bool ValidateResults()
        {
            bool result = true;
            ParallelOptions options = new System.Threading.Tasks.ParallelOptions();
            options.MaxDegreeOfParallelism = DegreeOfParallelism;

            var stopWatch = Stopwatch.StartNew();
            Parallel.ForEach(FilteredData, options, (string item, ParallelLoopState state) => {
                if (!item.StartsWith(SearchLine.ToUpper()))
                {
                    result = false;                    
                    Console.WriteLine("Invalid element: {0}", item);
                    state.Stop(); //quick validating - min 1 element is invalid. Without stop - show all invalid elements
                }
            }
            );            
            Console.WriteLine("Parallel ForEach (validating) execution time in seconds: {0}", stopWatch.Elapsed.TotalSeconds);

            stopWatch = Stopwatch.StartNew();
            foreach (string item in FilteredData)
            {
                if (!item.StartsWith(SearchLine.ToUpper()))
                {
                    result = false;
                    Console.WriteLine("Invalid element: {0}", item);
                    break; //quick validating - min 1 element is invalid. Without break - show all invalid elements
                }
            }
            Console.WriteLine("Sequetial foreach(validating) execution time in seconds: {0}", stopWatch.Elapsed.TotalSeconds);

            return result;
        }

        public bool FilterResults()
        {
            var stopWatch = Stopwatch.StartNew();
            FilteredData =  BaseData
                            .AsParallel()
                            .WithDegreeOfParallelism(DegreeOfParallelism)
                            .Where(e => e.StartsWith(SearchLine.ToUpper()));

            double secondsCount = stopWatch.Elapsed.TotalSeconds;

            Console.WriteLine("Search results:");
            IEnumerable<string> sortedData = FilteredData.OrderBy(e => e);
            foreach (var item in sortedData)
                Console.WriteLine(item);

            Console.WriteLine("Parallel LINK (filtering) execution time in seconds: {0}", secondsCount);            
            Console.WriteLine("Count of found elements: {0}", sortedData.Count());

            return ValidateResults();
        }
    }

    static class External
    {
        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == null)
            {
                return null;
            }

            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) => accumulator.SelectMany(
                    accseq => sequence,
                    (accseq, item) => accseq.Concat(new[] { item })));
        }
    }
}
