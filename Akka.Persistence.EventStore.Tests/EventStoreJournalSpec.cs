using Akka.Persistence.TestKit.Journal;
using Xunit;


namespace Akka.Persistence.EventStore.Tests
{
    [Collection("EventStore")]
    public class EventStoreJournalSpec : JournalSpec
    {
        private static readonly string SpecConfig = @"
            akka {
                stdout-loglevel = DEBUG
	            loglevel = DEBUG
                loggers = [""Akka.Logger.NLog.NLogLogger,Akka.Logger.NLog""]

persistence {
                publish-plugin-commands = on
                journal {
                    plugin = ""akka.persistence.journal.event-store""
                    event-store {
                        class = ""Akka.Persistence.EventStore.EventStoreJournal, Akka.Persistence.EventStore""
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                        
                        # the event store connection string
			            connection-string = ""ConnectTo=tcp://admin:changeit@127.0.0.1:1113;""

			            # name of the connection
			            connection-name = ""akka.net""
                    }
                }
            }
        }";

        // EventStore can't support writing an update that skips a sequence number
        protected override bool SupportsRejectingNonSerializableObjects => false;

        public EventStoreJournalSpec() : base(SpecConfig, "EventStoreJournalSpec")
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
