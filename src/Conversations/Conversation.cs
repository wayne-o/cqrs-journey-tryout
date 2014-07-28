using System;
using System.Collections.Generic;
using Conversations.Events;
using Infrastructure.EventSourcing;

namespace Conversations
{
    using Conversations.Dto;

    public class Conversation : EventSourced
    {
        public string Subject { get; set; }

        private void OnConversationStarted(ConversationStarted obj)
        {
            
        }

        public Conversation(Guid id, IEnumerable<IVersionedEvent> history) : base(id)
        {
            LoadFrom(history);
        }

        public Conversation(Guid id, string body, string creatorId, string subject)
            : base(id)
        {
            base.Handles<ConversationStarted>(this.OnConversationStarted);

            this.Update(new ConversationStarted(id, body, creatorId, subject));
        }
    }
}