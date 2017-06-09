using System;
using System.Collections.Generic;
using System.Data;

namespace Akka.Persistence.EventStore
{
    public interface ISnapshotEvent
    {
        
    }

    public interface IDeleteEvent : ISnapshotEvent
    {
        bool EventMatches(SnapshotMetadata snapshotMetadata);
    }

    public class Snapshot : ISnapshotEvent
    {
        public Object Data { get; }
        public SnapshotMetadata Metadata { get; }

        public Snapshot(Object data, SnapshotMetadata metadata)
        {
            Data = data;
            Metadata = metadata;
        }
    }

    public class DeleteEvent : IDeleteEvent
    {
        public long SequenceNumber { get; }
        public DateTime Timestamp { get; }

        public DeleteEvent(long sequenceNumber, DateTime timestamp)
        {
            SequenceNumber = sequenceNumber;
            Timestamp = timestamp;
        }

        public bool EventMatches(SnapshotMetadata snapshotMetadata)
        {
            return SequenceNumber == snapshotMetadata.SequenceNr || Timestamp == snapshotMetadata.Timestamp;
        }
    }

    public class DeleteCriteriaEvent : IDeleteEvent
    {
        public long MaxSequenceNumber { get; }
        public DateTime MaxTimestamp { get; }
        public long MinSequenceNumber { get; }
        public DateTime? MinTimestamp { get; }

        public DeleteCriteriaEvent(long maxSequenceNumber, DateTime maxTimestamp, long minSequenceNumber, DateTime? minTimestamp)
        {
            MaxSequenceNumber = maxSequenceNumber;
            MaxTimestamp = maxTimestamp;
            MinSequenceNumber = minSequenceNumber;
            MinTimestamp = minTimestamp;
        }

        public bool EventMatches(SnapshotMetadata metadata)
        {
            if (metadata.SequenceNr > MaxSequenceNumber || !(metadata.Timestamp <= MaxTimestamp) || metadata.SequenceNr < MinSequenceNumber)
                return false;
            if (!MinTimestamp.HasValue)
                return false;
            return metadata.Timestamp >= MinTimestamp.GetValueOrDefault();
        }
    }
}