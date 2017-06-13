using Akka.Persistence.TestKit.Snapshot;
using Xunit;

namespace Akka.Persistence.EventStore.Tests
{
    [Collection("EventStore")]
    public class EventStoreSnapshotSpec : SnapshotStoreSpec
    {
        private static readonly string SpecConfig = @"
            akka {
                stdout-loglevel = DEBUG
	            loglevel = DEBUG
                loggers = [""Akka.Logger.NLog.NLogLogger,Akka.Logger.NLog""]

                persistence {

                publish-plugin-commands = on
                snapshot-store {
                    plugin = ""akka.persistence.snapshot-store.event-store""
                    event-store {
                        class = ""Akka.Persistence.EventStore.EventStoreSnapshotStore, Akka.Persistence.EventStore""
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                        
                        # the event store connection string
                        connection-string = ""ConnectTo=tcp://admin:changeit@127.0.0.1:1113;""

                        # name of the connection
                        connection-name = ""akka.net""
                    }
                }
            }
        }";

        public EventStoreSnapshotSpec() : base(SpecConfig, "EventStoreSnapshotSpec")
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //cleanup
            StorageCleanup.Clean();
        }
    }


}
