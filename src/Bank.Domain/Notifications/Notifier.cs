using System.Collections.Generic;
using System.Linq;

namespace Bank.Domain.Notifications
{
    public class Notifier : INotifier
    {

        private readonly IList<Notification> _notifications;

        public Notifier()
        {
            _notifications = new List<Notification>();
        }

        public bool HasNotifications()
        {
            return _notifications.Any();
        }

        public IList<Notification> GetNotifications()
        {
            return _notifications;
        }

        public void Handle(Notification notification)
        {
            _notifications.Add(notification);
        }

    }
}
