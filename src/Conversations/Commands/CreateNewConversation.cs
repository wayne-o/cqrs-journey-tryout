using System;
using Infrastructure.Messaging;

namespace Conversations.Commands
{
    using Conversations.Dto;

    public class CreateNewConversation : ICommand
    {
        public CreateNewConversation(ConversationDto data)
        {
            this.Id = Guid.NewGuid();
            this.Conversation = data;
        }

        public CreateNewConversation()
        {
            
        }

        public ConversationDto Conversation { get; set; }

        public Guid Id { get; private set; }
    }
}
