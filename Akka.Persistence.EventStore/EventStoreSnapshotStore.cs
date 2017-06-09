using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.Snapshot;
using Akka.Serialization;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Akka.Persistence.EventStore
{
    public class EventStoreSnapshotStore : SnapshotStore
    {
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
        private readonly EventStorePersistenceExtension _extension;
        private Lazy<Task<IEventStoreConnection>> _connection;
        private EventStoreSerialization _serilization;
        private int _readBatchSize = 50;

        public EventStoreSnapshotStore()
        {
            _log = Context.GetLogger();
            _extension = EventStorePersistence.Instance.Apply(Context.System);
        }

        protected override void PreStart()
        {
            base.PreStart();

            _serilization = new EventStoreSerialization(Context.System.Serialization);

            _connection = new Lazy<Task<IEventStoreConnection>>(async () =>
            {
                IEventStoreConnection connection = EventStoreConnection.Create(_extension.EventStoreSnapshotSettings.ConnectionString, _extension.EventStoreSnapshotSettings.ConnectionName);
                await connection.ConnectAsync();
                return connection;
            });
        }

        private Task<IEventStoreConnection> GetConnection()
        {
            return _connection.Value;
        }

        protected override async Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            if (criteria == SnapshotSelectionCriteria.None) return null; // short circuit

            var connection = await GetConnection();
            var streamId = GetStreamName(persistenceId);
            var deletes = new List<IDeleteEvent>();
            var readFrom = StreamPosition.End;

            StreamEventsSlice slice;
            do
            {
                slice = await connection.ReadStreamEventsBackwardAsync(streamId, readFrom, _readBatchSize, false);
                foreach (var resolvedEvent in slice.Events)
                {
                    var snapshotEvent = _serilization.Deserialize<ISnapshotEvent>(resolvedEvent.Event);
                    switch (snapshotEvent)
                    {
                        case Snapshot s:
                            if (deletes.Any(d => d.EventMatches(s.Metadata))) break; // ignore this snapshot as it has been deleted
                            if (s.Metadata.SequenceNr < criteria.MinSequenceNr
                                || s.Metadata.Timestamp < criteria.MinTimestamp)
                            {   // remaining events can't fit in selection criteria
                                return null;
                            }
                            if (criteria.IsMatch(s.Metadata))
                            {
                                return new SelectedSnapshot(s.Metadata,s.Data);
                            }
                            break;
                        case IDeleteEvent de:
                            deletes.Add(de);
                            break;
                    }
                }
                readFrom = slice.NextEventNumber;

            } while (slice.IsEndOfStream == false);

            // didn't find a snapshot
            return null;
        }

        private static string GetStreamName(string persistenceId)
        {
            return $"snapshot-{persistenceId}";
        }

        protected override async Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            var connection = await GetConnection();
            var streamName = GetStreamName(metadata.PersistenceId);
            var eventData = _serilization.Serialize(new Snapshot(snapshot, metadata));

            await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);
        }

        protected override async Task DeleteAsync(SnapshotMetadata metadata)
        {
            await Delete(metadata.PersistenceId, new DeleteEvent(metadata.SequenceNr, metadata.Timestamp));
        }

        protected override async Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            await Delete(persistenceId,new DeleteCriteriaEvent(criteria.MaxSequenceNr, criteria.MaxTimeStamp, criteria.MinSequenceNr, criteria.MinTimestamp));
        }

        private async Task Delete(string persistanceId, IDeleteEvent deleteEvent)
        {
            var connection = await GetConnection();
            var streamName = GetStreamName(persistanceId);
            var eventData = _serilization.Serialize(deleteEvent);

            await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);
        }

    }
}