using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversations.Specs
{
    using System.Runtime;

    using Conversations.Api.ServiceInterface;
    using Conversations.Dto;

    using FluentAssertions;

    using Infrastructure.Messaging;

    using Moq;

    using NUnit.Framework;

    using Raven.Client;

    using ServiceStack.Host;

    [TestFixture]
    public class ConversationsServiceSpecs
    {
        [Test]
        public void Can_construct()
        {
            var commandBus = new Mock<ICommandBus>();
            var documentStore = new Mock<IDocumentStore>();

            var service = new ConversationsService(commandBus.Object, documentStore.Object);

            service.Should().NotBeNull();
        }

        [Test]
        public async void POST_calls_commandbus_SendAsync()
        {
            var commandBus = new Mock<ICommandBus>();
            var documentStore = new Mock<IDocumentStore>();

            var service = new ConversationsService(commandBus.Object, documentStore.Object)
                              {
                                  Request = new BasicRequest()
                              };

            await service.Post(new PostConversation
                                   {
                                       Data = new ConversationDto
                                                  {
                                                      Id = Guid.NewGuid().ToString()
                                                  }
                                   });

            commandBus.Verify(x => x.SendAsync(It.IsAny<Envelope<ICommand>>()), Times.Once);
        }

        [Test, Ignore]
        public async void POST_without_id_throws_argument_exception()
        {
            var commandBus = new Mock<ICommandBus>();
            var documentStore = new Mock<IDocumentStore>();

            var service = new ConversationsService(commandBus.Object, documentStore.Object)
            {
                Request = new BasicRequest()
            };

            service.Invoking(async x => await x.Post(new PostConversation { Data = new ConversationDto { Id = null } })).ShouldThrow<ArgumentException>();


            //Action action = async () => { await service.Post(new PostConversation { Data = new ConversationDto { Id = null } }); };
            //action.ShouldThrow<ArgumentException>();

        }
    }
}
