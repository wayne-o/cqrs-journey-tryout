//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Caching;
//using Conversations;
//using Conversations.Commands;
//using Funq;
//using Infrastructure;
//using Infrastructure.Azure;
//using Infrastructure.Azure.BlobStorage;
//using Infrastructure.Azure.EventSourcing;
//using Infrastructure.Azure.Instrumentation;
//using Infrastructure.Azure.Messaging;
//using Infrastructure.BlobStorage;
//using Infrastructure.EventSourcing;
//using Infrastructure.Messaging.Handling;
//using Infrastructure.Serialization;
//using Microsoft.WindowsAzure.Storage;

//namespace Sonatribe.CommandProcessor.WorkerRole
//{
//    partial class ConversationsCommandProcessor
//    {
//        private InfrastructureSettings azureSettings;
//        private ServiceBusConfig busConfig;

//        partial void OnCreating()
//        {
//            this.azureSettings = InfrastructureSettings.Read("Settings.xml");
//            this.busConfig = new ServiceBusConfig(this.azureSettings.ServiceBus);

//            busConfig.Initialize();
//        }

//        partial void OnCreateContainer(Container container)
//        {
//            var metadata = container.Resolve<IMetadataProvider>();
//            var serializer = container.Resolve<ITextSerializer>();

//            var blobStorageAccount = Microsoft.WindowsAzure.CloudStorageAccount.Parse(azureSettings.BlobStorage.ConnectionString);
//            container.Register<IBlobStorage>(new CloudBlobStorage(blobStorageAccount, azureSettings.BlobStorage.RootContainerName));

//            var commandBus = new CommandBus(new TopicSender(azureSettings.ServiceBus, Topics.Commands.Path), metadata, serializer);
//            var eventsTopicSender = new TopicSender(azureSettings.ServiceBus, Topics.Events.Path);
//            container.Register<IMessageSender>("events", eventsTopicSender);

//            var eventBus = new EventBus(eventsTopicSender, metadata, serializer);

//            var sessionlessCommandProcessor =
//                new Infrastructure.Azure.Messaging.Handling.CommandProcessor(new SubscriptionReceiver(azureSettings.ServiceBus, Topics.Commands.Path, Topics.Commands.Subscriptions.Sessionless, false, new SubscriptionReceiverInstrumentation(Topics.Commands.Subscriptions.Sessionless, this.instrumentationEnabled)), serializer);

//            var processors = new List<IProcessor> { sessionlessCommandProcessor };

//            container.Register<IEnumerable<IProcessor>>(processors);

//            var conversationsProcessor =
//            new Infrastructure.Azure.Messaging.Handling.CommandProcessor(
//                new SubscriptionReceiver(
//                    this.azureSettings.ServiceBus,
//                    Topics.Commands.Path,
//                    Topics.Commands.Subscriptions.CreateNewConversationCommandHandler,
//                    false,
//                    new SubscriptionReceiverInstrumentation(Topics.Commands.Subscriptions.CreateNewConversationCommandHandler, this.instrumentationEnabled)),
//                    serializer);

//            RegisterRepositories(container);

//            RegisterCommandHandlers(container, conversationsProcessor);
//        }

//        static void RegisterCommandHandlers(Container container, Infrastructure.Azure.Messaging.Handling.CommandProcessor conversationsProcessor)
//        {
//            var commandHandlers = container.Resolve<IEnumerable<ICommandHandler>>().ToList();
//            var firmawreCommandHandler = commandHandlers.First(x => x.GetType().IsAssignableFrom(typeof(CreateNewConversationCommandHandler)));
//            conversationsProcessor.Register(firmawreCommandHandler);
//        }

//        private void RegisterRepositories(Container container)
//        {
//            // repository
//            var eventSourcingAccount = Microsoft.WindowsAzure.CloudStorageAccount.Parse(this.azureSettings.EventSourcing.ConnectionString);
//            var conversationsEventStore = new EventStore(eventSourcingAccount, this.azureSettings.EventSourcing.ConversationsTableName);
//            container.Register<IEventStore>("conversations", conversationsEventStore);
//            container.Register<IPendingEventsQueue>("conversations", conversationsEventStore);

//            var cache = new MemoryCache("RepositoryCache");

//            //var conversationEventSourcedRepository = new AzureEventSourcedRepository<Conversation>(
//            //    container.ResolveNamed<IEventStore>("conversations"),
//            //    container.ResolveNamed<IEventStoreBusPublisher>("conversations"),
//            //    container.Resolve<ITextSerializer>(),
//            //    container.Resolve<IMetadataProvider>(),
//            //    cache);

//            //container.Register<IEventSourcedRepository<Conversation>>(conversationEventSourcedRepository);
//        }
//    }
//}