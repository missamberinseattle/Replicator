using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicator;

namespace ReplTests
{
    [TestClass]
    public class ChangeQueueTests
    {
        [TestMethod]
        public void QueueDequeue_QueueReplacement()
        {
            var queue = new ChangeQueue();

            queue.Queue(new FileChange(ChangeAction.None, "1", 10));
            queue.Queue(new FileChange(ChangeAction.None, "2", 10));
            queue.Queue(new FileChange(ChangeAction.None, "3", 10));
            
            queue.Queue(new FileChange(ChangeAction.None, "1", 10));

            Assert.IsTrue(queue.Count == 3);

            Assert.IsTrue(queue[0].File == "2");
            Assert.IsTrue(queue[1].File == "3");
            Assert.IsTrue(queue[2].File == "1");
            
        }
        [TestMethod]
        public void QueueDequeue_Three()
        {
            var queue = new ChangeQueue();

            queue.Queue(new FileChange(ChangeAction.None, "1", 10));
            queue.Queue(new FileChange(ChangeAction.None, "2", 10));
            queue.Queue(new FileChange(ChangeAction.None, "3", 10));

            Assert.IsTrue(queue.Count == 3);

            for (var xx = 0; xx < queue.Count; xx++)
            {
                Assert.IsTrue((xx + 1).ToString() == queue[xx].File);
            }

            FileChange change;

            change = queue.Dequeue();
            Assert.IsTrue(change.File == "1");
            Assert.IsTrue(queue.Count == 2);

            change = queue.Dequeue();
            Assert.IsTrue(change.File == "2");
            Assert.IsTrue(queue.Count == 1);

            change = queue.Dequeue();
            Assert.IsTrue(change.File == "3");
            Assert.IsTrue(queue.Count == 0);

        }
    }
}
