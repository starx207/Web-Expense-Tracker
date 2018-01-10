using System;
using System.Runtime.Serialization;

namespace ExpenseTracker.Exceptions
{
    [Serializable]
    public class IdNotFoundException : Exception
    {
        public IdNotFoundException() { }
        public IdNotFoundException(string message) : base(message) { }
        public IdNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected IdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NullIdException : Exception
    {
        public NullIdException() { }
        public NullIdException(string message) : base(message) { }
        public NullIdException(string message, Exception inner) : base(message, inner) { }
        protected NullIdException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IdMismatchException : Exception
    {
        public IdMismatchException() { }
        public IdMismatchException(string message) : base(message) { }
        public IdMismatchException(string message, Exception inner) : base(message, inner) { }
        protected IdMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}