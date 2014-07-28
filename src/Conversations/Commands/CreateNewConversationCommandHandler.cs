using System;
using Conversations.Events;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

namespace Conversations.Commands
{
    public class CreateNewConversationCommandHandler : ICommandHandler<CreateNewConversation>
    {
        private readonly IEventSourcedRepository<Conversation> _conversationRepository;

        public CreateNewConversationCommandHandler(IEventSourcedRepository<Conversation> conversationRepository)
        {
            this._conversationRepository = conversationRepository;
        }

        public void Handle(CreateNewConversation command)
        {
            var conversation = new Conversation(Guid.NewGuid(), command.Conversation.Body, command.Conversation.CreatorId, command.Conversation.Subject);

            _conversationRepository.Save(conversation, command.Id.ToString());
        }
    }
}