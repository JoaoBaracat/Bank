using System.Collections.Generic;

namespace Bank.Domain.Notifications
{
    public interface INotifier
    {

        bool HasNotifications();

        IList<Notification> GetNotifications();

        void Handle(Notification notification);

    }
}
