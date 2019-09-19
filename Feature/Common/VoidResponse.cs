using System;
using System.Collections.Generic;

namespace HAS.Registration.Feature
{
    public abstract class VoidResponse : IVoidResponse
    {
        public virtual bool HasErrors { get; private set; }
        public virtual List<string> Errors { get; private set; }
        public virtual string Message { get; private set; }
        public virtual string FormattedErrorString
        {
            get
            {
                return string.Join(" ", Errors);
            }
        }

        public virtual Exception Exception { get; private set; }

        public VoidResponse(string successMessage)
        {
            HasErrors = false;
            Message = successMessage;
            Errors = new List<string>();
        }

        public VoidResponse(bool hasError, string errorMessage)
        {
            HasErrors = hasError;
            Message = errorMessage;
            Errors = new List<string>();
            this.AddErrorMessage(errorMessage);
        }

        public VoidResponse(bool hasError, string errorMessage, Exception exception)
            : this(hasError, errorMessage)
        {
            Exception = exception;
        }

        public void AddErrorMessage(string message)
        {
            Errors.Add(message);
        }
    }
}
