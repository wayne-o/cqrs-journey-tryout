using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

namespace Sonatribe.CommandProcessor.WorkerRole
{
    using Castle.Core;

    using Conversations.Common.Configuration;

    using Raven.Client;
    using Raven.Client.Document;

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

            ConfigureDb(_container);

            // infrastructure
            _container.Register(Component.For<ITextSerializer>().Instance(new JsonTextSerializer()));
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

            _container.Register(Component.For<IProcessor>().Instance(commandProcessor).Named("CommandProcessor"));
            _container.Register(Component.For<IProcessor>().Instance(eventProcessor).Named("EventProcessor"));

            var sqlMessageLog = new SqlMessageLog("MessageLog", serializer, metadata);
            _container.Register(Component.For<SqlMessageLog>().Instance(sqlMessageLog));

            var sqlMessageLogHandler = new SqlMessageLogHandler(sqlMessageLog);

            _container.Register(Component.For<IEventHandler>().Instance(sqlMessageLogHandler));

            RegisterRepository(_container);

             _container.Register(Component.For<ICommandHandler>().Instance(sqlMessageLogHandler).Named("sqlMessageLogHandler"));

            _container.Register(
                 Types
                   .FromAssembly(typeof(Conversation).Assembly)
                   .BasedOn(typeof(ICommandHandler)).WithServiceAllInterfaces().Configure(x => x.LifeStyle.Is(LifestyleType.Transient))
               );

            _container.Register(
                Types
                  .FromAssembly(typeof(ConversationDenormalizer).Assembly)
                  .BasedOn(typeof(IEventHandler)).WithServiceAllInterfaces().Configure(x => x.LifeStyle.Is(LifestyleType.Transient))
              );

            RegisterEventHandlers(_container, eventProcessor);
            RegisterCommandHandlers(_container);

            return _container;
        }

        private void ConfigureDb(WindsorContainer container)
        {
            IDocumentStore store = new DocumentStore
            {
                Url = AppSettings.GetConfigurationString("SonatribeConnectionString"),
                DefaultDatabase = AppSettings.GetConfigurationString("SonatribeConnectionStringDBName")
            }.Initialize();

            store.Conventions.IdentityPartsSeparator = "-";

            container.Register(Component.For<IDocumentStore>().Instance(store).LifestyleTransient());
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
            var eventHandlers = container.ResolveAll<IEventHandler>();

            if (eventHandlers != null && eventHandlers.Any())
            {
                foreach (var eventHandler in eventHandlers)
                {
                    eventProcessor.Register(eventHandler);
                }
            }
        }

        private void RegisterRepository(WindsorContainer container)
        {
            container.Register(Component.For<EventStoreDbContext>().ImplementedBy<EventStoreDbContext>().DependsOn(Dependency.OnValue("nameOrConnectionString", "EventStore")).LifestyleTransient());
            Func<EventStoreDbContext> f = container.Resolve<EventStoreDbContext>;
            container.Register(Component.For<Func<EventStoreDbContext>>().Instance(f));
            container.Register(Component.For(typeof(IEventSourcedRepository<>)).LifestyleTransient().ImplementedBy(typeof(SqlEventSourcedRepository<>)));
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
