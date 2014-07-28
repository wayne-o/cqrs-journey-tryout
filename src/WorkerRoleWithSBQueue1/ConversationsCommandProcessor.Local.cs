using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conversations;
using Funq;
using Infrastructure;
using Infrastructure.BlobStorage;
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
    public partial class ConversationsCommandProcessor
    {
        
    }
}
