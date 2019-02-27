using System;
using System.Collections.Generic;

namespace Replicator
{
    public class ChangeQueue : List<FileChange>
    {
        public ChangeQueue() : base()
        {
        }

        private ChangeQueue(int capacity) : base(capacity)
        {

        }

        private ChangeQueue(IEnumerable<FileChange> collection) : base(collection)
        {

        }


        private static readonly object _padlock = new object();

    private new void Add(FileChange fileChange)
        {
            base.Add(fileChange);
        }

        public void Queue(FileChange fileChange)
        {
            lock (_padlock)
            {
                for (var xx = Count - 1; xx > -1; xx--)
                {
                    if (this[xx].File == fileChange.File)
                    {
                        RemoveAt(xx);
                    }
                }

                Add(fileChange);
            }
        }

        public FileChange Dequeue()
        {
            lock (_padlock)
            {
                var value = FileChange.Empty;

                if (Count > 0)
                {
                    value = this[0];
                    RemoveAt(0);
                }

                return value;
            }
        }
    }
}