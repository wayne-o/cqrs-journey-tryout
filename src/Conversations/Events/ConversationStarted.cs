// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConversationStarted.cs" company="">
//   
// </copyright>
// <summary>
//   The conversation started.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Conversations.Events
{
    using System;

    using Infrastructure.EventSourcing;

    /// <summary>
    /// The conversation started.
    /// </summary>
    public class ConversationStarted : VersionedEvent
    {
        #region Fields

        /// <summary>
        /// The subject.
        /// </summary>
        public readonly string Subject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationStarted"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        /// <param name="creatorId">
        /// The creator id.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        public ConversationStarted(Guid id, string body, string creatorId, string subject)
        {
            this.Id = id;
            this.Body = body;
            this.CreatorId = creatorId;
            this.Subject = subject;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the creator id.
        /// </summary>
        public string CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        #endregion
    }
}