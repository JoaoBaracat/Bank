using Bank.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Bank.Api.Controllers
{
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly INotifier _notifier;
        private readonly ILogger<MainController> _logger;


        public MainController(INotifier notifier, ILogger<MainController> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (!IsOperationValid())
            {
                var errors = _notifier.GetNotifications().Select(n => n.Message);
                if (result != null)
                {
                    _logger.LogWarning($"Invalid operation with object: {JsonConvert.SerializeObject(result)}");
                }
                _logger.LogWarning($"Invalid operation with errors: {JsonConvert.SerializeObject(errors)}");
                return NotFound(new { success = false, errors = errors, data = result });
            }

            _logger.LogInformation($"Successful operation: {JsonConvert.SerializeObject(result)}");
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
