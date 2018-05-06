using System;
using System.Runtime.Serialization;

namespace ExpenseTracker.Exceptions
{
    [Serializable]
    public class ExpenseTrackerException : Exception
    {
        public ExpenseTrackerException() { }
        public ExpenseTrackerException(string message) : base(message) { }
        public ExpenseTrackerException(string message, Exception inner) : base(message, inner) { }
        protected ExpenseTrackerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IdNotFoundException : ExpenseTrackerException
    {
        public IdNotFoundException() { }
        public IdNotFoundException(string message) : base(message) { }
        public IdNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected IdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NullIdException : ExpenseTrackerException
    {
        public NullIdException() { }
        public NullIdException(string message) : base(message) { }
        public NullIdException(string message, Exception inner) : base(message, inner) { }
        protected NullIdException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IdMismatchException : ExpenseTrackerException
    {
        public IdMismatchException() { }
        public IdMismatchException(string message) : base(message) { }
        public IdMismatchException(string message, Exception inner) : base(message, inner) { }
        protected IdMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ConcurrencyException : ExpenseTrackerException
    {
        public ConcurrencyException() { }
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception inner) : base(message, inner) { }
        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ModelValidationException : ExpenseTrackerException
    {
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; } = string.Empty;
        public ModelValidationException() { }
        public ModelValidationException(string propertyName, string propertyValue) {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
        public ModelValidationException(string propertyName, string propertyValue, string message) : base(message) {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
        public ModelValidationException(string propertyName, string propertyValue, string message, Exception inner) : base(message, inner) {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
        public ModelValidationException(string message) : base(message) { }
        public ModelValidationException(string message, Exception inner) : base(message, inner) { }
        protected ModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class InvalidDateExpection : ExpenseTrackerException
    {
        public InvalidDateExpection() { }
        public InvalidDateExpection(string message) : base(message) { }
        public InvalidDateExpection(string message, Exception inner) : base(message, inner) { }
        protected InvalidDateExpection(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}