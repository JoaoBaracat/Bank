using AutoMapper;
using Bank.Api.Models.Transaction.Commands;
using Bank.Api.Models.Transaction.Queries;
using Bank.Domain.Apps;
using Bank.Domain.Entities;
using Bank.Domain.Enums;
using Bank.Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Bank.Api.Controllers
{
    [Route("api/fund-transfer")]
    [ApiController]
    //[Authorize]
    public class TransactionController : MainController
    {
        private readonly IMapper _mapper;
        private readonly ITransactionApp _transactionApp;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionApp transactionApp, IMapper mapper, INotifier notifier, ILogger<TransactionController> logger) : base(notifier, logger)
        {
            _mapper = mapper;
            _transactionApp = transactionApp;
            _logger = logger;
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult> GetTransactionStatusById(Guid id)
        {
            _logger.LogInformation($"Request for new transaction status with id: {id}");
            var transaction = await _transactionApp.GetById(id);
            if (transaction == null)
            {
                return CustomResponse();
            }

            if (transaction.Status == (int)TransactionStatusEnum.TransactionStatus.Error)
            {
                return CustomResponse(_mapper.Map<ResponseTransactionErrorQuery>(transaction));
            }
            else
            {
                return CustomResponse(_mapper.Map<ResponseTransactionStatusQuery>(transaction));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseTransactionQuery>> CreateTransaction(CreateTransactionCommand createTransactionCommand)
        {
            _logger.LogInformation($"Request for new transaction with transaction: {JsonConvert.SerializeObject(createTransactionCommand)}");
            if (!IsModelValid())
            {                
                return CustomResponse(createTransactionCommand);
            }

            var transactionResponse = _mapper.Map<ResponseTransactionQuery>(await _transactionApp.Create(_mapper.Map<Transaction>(createTransactionCommand)));
            return CustomResponse(transactionResponse);
        }

    }
}
