using Infrastructure.Messaging.Handling;

namespace Conversations.Events
{
    using System;

    using Raven.Client;

    public class ConversationDenormalizer : IEventHandler<ConversationStarted>
    {
        private readonly IDocumentStore documentStore;

        public ConversationDenormalizer(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }

        public void Handle(ConversationStarted @event)
        {
            var conversation = new DenormalizedConversation
                                   {
                                       Body = @event.Body,
                                       Subject = @event.Subject
                                   };

            using (var session = documentStore.OpenSession())
            {
                session.Store(conversation);

                session.SaveChanges();
            }
        }
    }

    public class DenormalizedConversation
    {
        public string Body { get; set; }

        public string Subject { get; set; }
    }
}