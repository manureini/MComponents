using System;

namespace MComponents
{
    public class UserMessageException : Exception
    {
        public UserMessageException(string pMessage) : base(pMessage)
        {
        }
    }
}
