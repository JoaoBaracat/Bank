using Bank.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bank.Api.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly INotifier _notifier;


        public MainController(INotifier notifier)
        {
            _notifier = notifier;
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (!IsOperationValid())
            {
                return BadRequest(new { success = false, errors = _notifier.GetNotifications().Select(n => n.Message), data = result });
            }

            return Ok(new { success = true, data = result });
        }

        protected bool IsOperationValid()
        {

            return !_notifier.HasNotifications();
        }

        protected bool IsModelValid()
        {
            if (ModelState.IsValid)
            {
                return true;
            }

            NotifyModelStateErrors();

            return false;
        }

        protected void NotifyModelStateErrors()
        {
            IEnumerable<ModelError> errors = ModelState.Values.SelectMany(e => e.Errors);

            foreach (var error in errors)
            {
                string errorMessage = error.Exception == null ? error.ErrorMessage : error.Exception.Message;

                NotifyError(errorMessage);
            }
        }

        protected void NotifyError(string errorMessage)
        {
            _notifier.Handle(new Notification(errorMessage));

        }
    }
}
