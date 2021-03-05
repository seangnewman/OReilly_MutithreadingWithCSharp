using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter7_PLINQ
{
    public class StringPartitioner : Partitioner<string>
    {
        private readonly IEnumerable<string> _data;
        public StringPartitioner(IEnumerable<string> data)
        {
            _data = data;
        }

        public override bool SupportsDynamicPartitions => false;

        public override IList<IEnumerator<string>> GetPartitions(int partitionCount)
        {
            var result = new List<IEnumerator<string>>(partitionCount);

            for (int i = 1; i <= partitionCount; i++)
            {
                result.Add(CreateEnumerator(i, partitionCount));
            }
            return result;
        }

        private IEnumerator<string> CreateEnumerator(int partitionNumber, int partitionCount)
        {
            int evenPartitions = partitionCount / 2;
            bool isEven = partitionNumber % 2 == 0;
            int step = isEven ? evenPartitions : partitionCount - evenPartitions;

            int startIndex = partitionNumber / 2 + partitionNumber % 2;

            var q = _data
                        .Where( v => !(v.Length % 2 == 0 ^ isEven)  || partitionCount == 1).Skip(startIndex - 1);

            return q.Where((x, i) => i % step == 0).GetEnumerator();
        }
    }
}
