using System;

namespace Harald.WebApi.Infrastructure.Messaging
{
    public class MessagingException : Exception
    {
        public MessagingException(string message) : base(message)
        {
        }
    }
        
    public class EventTypeNotFoundException : MessagingException
    {
        public EventTypeNotFoundException(string message) : base(message)
        {
        }
    }

    public class EventMessageIncomprehensible : MessagingException
    {
        public EventMessageIncomprehensible(string message) : base(message)
        {
        }
    }

    public class EventHandlerNotFoundException : MessagingException
    {
        public EventHandlerNotFoundException(string message) : base(message)
        {
        }
    }
}