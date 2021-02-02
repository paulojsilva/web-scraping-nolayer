using Flunt.Notifications;
using System.Collections.Generic;

namespace Application.Common
{
    public class ServiceResult
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public StatusResult Status { get; set; }
        public string StatusDescription => Status.ToString();

        public enum StatusResult
        {
            Ok,
            Error,
            Warning
        }
    }

    public class ServiceTResult<T> : ServiceResult
    {
        public T Data { get; set; }
    }
}
