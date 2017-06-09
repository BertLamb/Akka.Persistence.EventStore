using System;

namespace Akka.Persistence.EventStore
{
    public static class SnapshotSelectionCriteriaExtensions
    {
        public static bool IsMatch(this SnapshotSelectionCriteria criteria, SnapshotMetadata metadata)
        {
            if (metadata.SequenceNr > criteria.MaxSequenceNr || !(metadata.Timestamp <= criteria.MaxTimeStamp) || metadata.SequenceNr < criteria.MinSequenceNr)
                return false;
            ;
            if (!criteria.MinTimestamp.HasValue)
                return false;
            return metadata.Timestamp >= criteria.MinTimestamp.GetValueOrDefault();
        }
    }
}