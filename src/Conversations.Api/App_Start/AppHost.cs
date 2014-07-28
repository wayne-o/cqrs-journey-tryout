using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Conversations.Api.App_Start;
using Funq;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(AppHost), "Start")]
namespace Conversations.Api.App_Start
{
    using Raven.Client;
    using Raven.Client.Document;
    using Conversations.Common.Configuration;


    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("Sonatribe Conversations Api wikiwkikiwah", typeof(ServiceInterface.ConversationsService).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = DateHandler.ISO8601;
            ConfigureContainer(container);
        }

        public override void OnUncaughtException(IRequest httpReq, IResponse httpRes, string operationName, Exception ex)
        {
            base.OnUncaughtException(httpReq, httpRes, operationName, ex);
        }

        public override object OnServiceException(IRequest httpReq, object request, Exception ex)
        {
            return base.OnServiceException(httpReq, request, ex);
        }


        private void ConfigureContainer(Container container)
        {
            var serializer = new JsonTextSerializer();
            var messageSender = new MessageSender(Database.DefaultConnectionFactory, "conversations", "SqlBus.Commands");
            //var commandBus = new CommandBus(messageSender, serializer);

            container.Register<ITextSerializer>(serializer);
            container.Register<IMessageSender>(messageSender);
            container.RegisterAs<CommandBus, ICommandBus>();
            this.ConfigureDb(container);
        }

        private void ConfigureDb(Container container)
        {
            IDocumentStore store = new DocumentStore
            {
                Url = AppSettings.GetConfigurationString("SonatribeConnectionString"),
                DefaultDatabase = AppSettings.GetConfigurationString("SonatribeConnectionStringDBName")
            }.Initialize();

            store.Conventions.IdentityPartsSeparator = "-";

            container.Register(store);
        }

        public static void Start()
        {
            var appHost = new AppHost();
            appHost.Init();
        }
    }
}