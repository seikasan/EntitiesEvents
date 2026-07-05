using EntitiesEvents;
using NUnit.Framework;
using Unity.Collections;

namespace EntitiesEvents.Tests
{
    public sealed class EventsTests
    {
        struct TestEvent
        {
            public int Value;
        }

        [Test]
        public void CachedReaderReadsSameFrameEventsOnce()
        {
            var events = new Events<TestEvent>(1, Allocator.Temp);
            try
            {
                var reader = events.GetReader();
                var writer = events.GetWriter();

                writer.Write(new TestEvent { Value = 10 });

                Assert.AreEqual(10, Sum(ref reader));
                Assert.AreEqual(0, Count(ref reader));
            }
            finally
            {
                events.Dispose();
            }
        }

        [Test]
        public void CachedReaderReadsPreviousFrameEvents()
        {
            var events = new Events<TestEvent>(1, Allocator.Temp);
            try
            {
                var reader = events.GetReader();
                var writer = events.GetWriter();

                writer.Write(new TestEvent { Value = 20 });
                events.Update();

                Assert.AreEqual(20, Sum(ref reader));
            }
            finally
            {
                events.Dispose();
            }
        }

        [Test]
        public void UnreadEventsExpireAfterTwoUpdates()
        {
            var events = new Events<TestEvent>(1, Allocator.Temp);
            try
            {
                var reader = events.GetReader();
                var writer = events.GetWriter();

                writer.Write(new TestEvent { Value = 30 });
                events.Update();
                events.Update();

                Assert.AreEqual(0, Count(ref reader));
            }
            finally
            {
                events.Dispose();
            }
        }

        [Test]
        public void MultipleReadersKeepIndependentReadPositions()
        {
            var events = new Events<TestEvent>(1, Allocator.Temp);
            try
            {
                var readerA = events.GetReader();
                var readerB = events.GetReader();
                var writer = events.GetWriter();

                writer.Write(new TestEvent { Value = 1 });
                writer.Write(new TestEvent { Value = 2 });

                Assert.AreEqual(3, Sum(ref readerA));
                Assert.AreEqual(3, Sum(ref readerB));
                Assert.AreEqual(0, Count(ref readerA));
                Assert.AreEqual(0, Count(ref readerB));
            }
            finally
            {
                events.Dispose();
            }
        }

        [Test]
        public void EnsureCapacityGrowsBothBuffers()
        {
            var events = new Events<TestEvent>(1, Allocator.Temp);
            try
            {
                events.EnsureCapacity(128);

                Assert.GreaterOrEqual(events.Capacity, 128);
                Assert.AreEqual(0, events.CurrentFrameCount);
                Assert.GreaterOrEqual(events.RemainingCurrentFrameCapacity, 128);
            }
            finally
            {
                events.Dispose();
            }
        }

        static int Count(ref EventReader<TestEvent> reader)
        {
            var count = 0;
            foreach (var _ in reader.Read())
            {
                count++;
            }

            return count;
        }

        static int Sum(ref EventReader<TestEvent> reader)
        {
            var sum = 0;
            foreach (var value in reader.Read())
            {
                sum += value.Value;
            }

            return sum;
        }
    }
}
