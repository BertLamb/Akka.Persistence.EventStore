using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Akka.Persistence.EventStore
{
    public class EventStoreJournalSettings : JournalSettings
    {
        public const string ConfigPath = "akka.persistence.journal.event-store";

        public EventStoreJournalSettings(Config config)
            : base(config)
        {
        }
    }

    public class EventStoreSnapshotSettings : SnapshotStoreSettings
    {
        public const string ConfigPath = "akka.persistence.snapshot-store.event-store";

        public EventStoreSnapshotSettings(Config config)
            : base(config)
        {
        }
    }

    /// <summary>
    /// An actor system extension initializing support for EventStore persistence layer.
    /// </summary>
    public class EventStorePersistenceExtension : IExtension
    {
        /// <summary>
        /// Journal-related settings loaded from HOCON configuration.
        /// </summary>
        public readonly EventStoreJournalSettings EventStoreJournalSettings;

        /// <summary>
        /// Snapshot store related settings loaded from HOCON configuration.
        /// </summary>
        public readonly EventStoreSnapshotSettings EventStoreSnapshotSettings;


        public EventStorePersistenceExtension(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(EventStorePersistence.DefaultConfiguration());
            
            // hack alert 
            var serializer = new NewtonSoftJsonEventStoreSerializer(system);
            system.Serialization.AddSerializer(serializer);
            system.Serialization.AddSerializationMap(typeof(ISnapshotEvent), serializer);
            system.Serialization.AddSerializationMap(typeof(Persistent), serializer);


            EventStoreJournalSettings = new EventStoreJournalSettings(system.Settings.Config.GetConfig(EventStoreJournalSettings.ConfigPath));
            EventStoreSnapshotSettings = new EventStoreSnapshotSettings(system.Settings.Config.GetConfig(EventStoreSnapshotSettings.ConfigPath));
        }
    }

    /// <summary>
    /// Singleton class used to set up EventStorePersistence for akka persistence plugin.
    /// </summary>
    public class EventStorePersistence : ExtensionIdProvider<EventStorePersistenceExtension>
    {
        public static readonly EventStorePersistence Instance = new EventStorePersistence();

        /// <summary>
        /// Initializes a Table Storage persistence plugin inside provided <paramref name="actorSystem"/>.
        /// </summary>
        public static void Init(ActorSystem actorSystem)
        {
            Instance.Apply(actorSystem);
        }

        private EventStorePersistence() { }

        /// <summary>
        /// Creates an actor system extension for akka persistence Azure Storage support.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override EventStorePersistenceExtension CreateExtension(ExtendedActorSystem system)
        {
            return new EventStorePersistenceExtension(system);
        }

        /// <summary>
        /// Returns a default configuration for akka persistence EventStore journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<EventStorePersistence>("Akka.Persistence.EventStore.reference.conf");
        }
    }
}
