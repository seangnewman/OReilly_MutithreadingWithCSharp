using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter10_ParallelProgrammingPatterns
{
    public static class Extensions
    {
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
                                this ParallelQuery<TSource> source, 
                                Func<TSource, IEnumerable<TMapped>> map,
                                Func<TMapped, TKey> keySelector,
                                Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce  )
                                {
                                    return source.SelectMany(map).GroupBy(keySelector).SelectMany(reduce);
                                 }
                                  public static IEnumerable<string> EnumLines(this StringReader reader)
                                    {
                                        while (true)
                                        {
                                                string line = reader.ReadLine();
                                                if (null == line)
                                                {
                                                    yield break;
                                                }
                                                yield return line;
                                        }

                                    }


    }
}
