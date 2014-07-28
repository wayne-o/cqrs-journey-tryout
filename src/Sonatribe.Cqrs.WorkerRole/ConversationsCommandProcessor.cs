using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Conversations;
using Conversations.Commands;
using Conversations.Events;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Serialization;
using Infrastructure.Sql.BlobStorage;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Handling;
using Infrastructure.Sql.Messaging.Implementation;

namespace Sonatribe.Cqrs.WorkerRole
{
    public sealed partial class ConversationsCommandProcessor : IDisposable
    {
        private WindsorContainer _container;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<IProcessor> _processors;
        private bool _instrumentationEnabled;
        private bool instrumentationEnabled;

        public ConversationsCommandProcessor(bool instrumentationEnabled = false)
        {
            _instrumentationEnabled = instrumentationEnabled;

            //OnCreating();

            _cancellationTokenSource = new CancellationTokenSource();
            _container = CreateContainer();

            _processors = _container.ResolveAll<IProcessor>().ToList();
        }

        private WindsorContainer CreateContainer()
        {
            _container = new WindsorContainer();

            _container.Register(Component.For<object>().Instance(new object()));

            // infrastructure
            _container.Register(Component.For<ITextSerializer>().Instance(new ServicestackTextSerializer()));
            _container.Register(Component.For<IMetadataProvider>().Instance(new StandardMetadataProvider()));

            var serializer = _container.Resolve<ITextSerializer>();
            var metadata = _container.Resolve<IMetadataProvider>();

            _container.Register(Component.For<SqlBlobStorage>().Instance(new SqlBlobStorage("")));

            var commandBus = new CommandBus(new MessageSender(Database.DefaultConnectionFactory, "conversations", "SqlBus.Commands"), serializer);
            var eventBus = new EventBus(new MessageSender(Database.DefaultConnectionFactory, "conversations", "SqlBus.Events"), serializer);


            var commandProcessor = new Infrastructure.Sql.Messaging.Handling.CommandProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "conversations", "SqlBus.Commands"), serializer);
            var eventProcessor = new EventProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "conversations", "SqlBus.Events"), serializer);

            _container.Register(Component.For<ICommandBus>().Instance(commandBus));
            _container.Register(Component.For<IEventBus>().Instance(eventBus));
            _container.Register(Component.For<ICommandHandlerRegistry>().Instance(commandProcessor));
            _container.Register(Component.For<IEventHandlerRegistry>().Instance(eventProcessor));

            _container.Register(Component.For<IProcessor>().Instance(commandProcessor));
            _container.Register(Component.For<IProcessor>().Instance(eventProcessor));


            //_container.Register<IEnumerable<IProcessor>>(c =>
            //    new List<IProcessor>
            //    {
            //        new Infrastructure.Sql.Messaging.Handling.CommandProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "conversations", "SqlBus.Commands"), c.Resolve<ITextSerializer>()), 
            //        new EventProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "conversations", "SqlBus.Events"), c.Resolve<ITextSerializer>())
            //    }).ReusedWithin(ReuseScope.Request);

            // Event log database and handler.
            var sqlMessageLog = new SqlMessageLog("MessageLog", serializer, metadata);
            _container.Register(Component.For<SqlMessageLog>().Instance(sqlMessageLog));

            var sqlMessageLogHandler = new SqlMessageLogHandler(sqlMessageLog);

            _container.Register(Component.For<IEventHandler>().Instance(sqlMessageLogHandler));

            RegisterRepository(_container);

            //var commandHandlers = new List<ICommandHandler>();
            //commandHandlers.Add(new CreateNewConversationCommandHandler(_container.Resolve<IEventSourcedRepository<Conversation>>()));
            //commandHandlers.Add(sqlMessageLogHandler);
            //_container.Register<IEnumerable<ICommandHandler>>(commandHandlers);

            _container.Register(
                Component.For<ICommandHandler>()
                    .Instance(
                        new CreateNewConversationCommandHandler(
                            _container.Resolve<IEventSourcedRepository<Conversation>>())));
            _container.Register(Component.For<ICommandHandler>().Instance(sqlMessageLogHandler));


            RegisterEventHandlers(_container, eventProcessor);
            RegisterCommandHandlers(_container);

            return _container;
        }

        public void Start()
        {
            _processors.ForEach(p => p.Start());
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();

            _processors.ForEach(p => p.Stop());
        }

        //partial void OnCreating();
        //partial void OnCreateContainer(Container container);

        public void Dispose()
        {
            _container.Dispose();
            _cancellationTokenSource.Dispose();
        }

        private void RegisterEventHandlers(WindsorContainer container, EventProcessor eventProcessor)
        {
            eventProcessor.Register(new ConversationDenormalizer());

            //eventProcessor.Register(container.Resolve<SqlMessageLogHandler>());
        }

        private void RegisterRepository(WindsorContainer container)
        {
            // repository
            container.Register(Component.For<EventStoreDbContext>().Instance(new EventStoreDbContext("EventStore")));
            //TODO: for each AR register: 
            //container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());
            //NO open generics in Funq

            container.Register(Component.For<IEventSourcedRepository<Conversation>>().Instance(new SqlEventSourcedRepository<Conversation>(
                container.Resolve<IEventBus>(), container.Resolve<ITextSerializer>(), container.Resolve<EventStoreDbContext>)));
        }

        static void RegisterCommandHandlers(WindsorContainer container)
        {
            var commandHandlerRegistry = container.Resolve<ICommandHandlerRegistry>();

            var commandHandlers = container.ResolveAll<ICommandHandler>();

            if (commandHandlers != null && commandHandlers.Any())
            {
                foreach (var commandHandler in commandHandlers)
                {
                    commandHandlerRegistry.Register(commandHandler);
                }
            }
        }
    }
}
