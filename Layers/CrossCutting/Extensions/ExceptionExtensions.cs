namespace System
{
    public static class ExceptionExtensions
    {
        public static string GetMessageConcatenatedWithInner(this Exception ex, string message = null)
        {
            if (ex == null) return string.Empty;

            var updateMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(message))
            {
                updateMessage = ex.Message;
            }
            else if (!message.ContainsInvariant(ex.Message))
            {
                updateMessage = message + " " + ex.Message;
            }
            else
            {
                updateMessage = message;
            }

            if (ex.InnerException != null)
                return GetMessageConcatenatedWithInner(ex.InnerException, updateMessage);

            return updateMessage;
        }

        public static bool ContainsMessageOnException(this Exception ex, string msg)
        {
            if (ex == null) return false;

            if (!string.IsNullOrWhiteSpace(msg))
            {
                var contains = ex.Message.ContainsInvariant(msg);
                if (contains) return true;
            }

            if (ex.InnerException != null)
                return ContainsMessageOnException(ex.InnerException, msg);

            return false;
        }

        public static bool ExceptionOfType(this Exception ex, Type expect)
        {
            if (ex == null) return false;

            if (ex.GetType() == expect)
                return true;

            if (ex.InnerException != null)
                return ExceptionOfType(ex.InnerException, expect);

            return false;
        }

        public static bool ExceptionOfType(this Exception ex, string expectTypeName)
        {
            if (ex == null) return false;

            if (ex.GetType().Name == expectTypeName)
                return true;

            if (ex.InnerException != null)
                return ExceptionOfType(ex.InnerException, expectTypeName);

            return false;
        }

        public static bool ExceptionOfTypeAndContains(this Exception ex, Type expect, string msg)
        {
            if (ExceptionOfType(ex, expect))
            {
                return ContainsMessageOnException(ex, msg);
            }

            return false;
        }

        public static TException GetExceptionOfType<TException>(this Exception ex) where TException : class
        {
            if (ex == null) return null;

            if (ex.GetType() == typeof(TException))
                return ex as TException;

            if (ex.InnerException != null)
                return GetExceptionOfType<TException>(ex.InnerException);

            return null;
        }
    }
}