using AutoMapper;
using Bank.Api.Models.Transaction.Commands;
using Bank.Api.Models.Transaction.Queries;
using Bank.Domain.Apps;
using Bank.Domain.Entities;
using Bank.Domain.Enums;
using Bank.Domain.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bank.Api.Controllers
{
    [Route("api/fund-transfer")]
    [ApiController]
    [Authorize]
    public class TransactionController : MainController
    {
        private readonly IMapper _mapper;
        private readonly ITransactionApp _transactionApp;

        public TransactionController(ITransactionApp transactionApp, IMapper mapper, INotifier notifier) : base(notifier)
        {
            _mapper = mapper;
            _transactionApp = transactionApp;
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult> GetTransactionStatusById(Guid id)
        {
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
            if (!IsModelValid())
            {
                return CustomResponse(createTransactionCommand);
            }

            var transactionResponse = _mapper.Map<ResponseTransactionQuery>(await _transactionApp.Create(_mapper.Map<Transaction>(createTransactionCommand)));
            return CustomResponse(transactionResponse);
        }

    }
}
