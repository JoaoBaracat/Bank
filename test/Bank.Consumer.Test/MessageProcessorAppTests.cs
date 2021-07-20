using Bank.Domain.Apps.Services;
using Bank.Domain.Entities;
using Bank.Infra.Consumers.App;
using Bank.Infra.Consumers.Models.APIConta;
using Moq;
using System.Threading.Tasks;
using Xunit;
using static Bank.Infra.Consumers.Models.Enums.ProcessorResultEnum;

namespace Bank.Consumer.Test
{
    public class MessageProcessorAppTests
    {
        private Mock<IAPIContaClient> _apiContaMock;
        private Transaction _transaction;

        public MessageProcessorAppTests()
        {
            _transaction = new Transaction() { AccountDestination = "456", AccountOrigin = "123" };
            _apiContaMock = new Mock<IAPIContaClient>();
        }

        [Fact]
        public async Task ShouldAccountBeValid()
        {
            var messageProcessorApp = new MessageProcessorApp(_apiContaMock.Object);
            _apiContaMock.Setup(x => x.GetAccountByNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(await Task.FromResult(new Account() { AccountNumber = "123", Balance = 15.22M, Id = 1 }));

            var result = await messageProcessorApp.ValidateAccounts(_transaction);
            
            Assert.Equal(AccountsResultEnum.AccountsOk, result);
            _apiContaMock.Verify(x => x.GetAccountByNumberAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldAccountBeNotAllowed()
        {
            var messageProcessorApp = new MessageProcessorApp(_apiContaMock.Object);
            _apiContaMock.Setup(x => x.GetAccountByNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(await Task.FromResult(new Account() { Id = 0 }));

            var result = await messageProcessorApp.ValidateAccounts(_transaction);

            Assert.Equal(AccountsResultEnum.NotAllowed, result);
            _apiContaMock.Verify(x => x.GetAccountByNumberAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldTranferFunds()
        {
            var messageProcessorApp = new MessageProcessorApp(_apiContaMock.Object);
            _apiContaMock.Setup(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()))
                .ReturnsAsync(await Task.FromResult(new BalanceAdjustmentResponse() { Response = "Success" }));

            var result = await messageProcessorApp.TransferFunds(_transaction);

            Assert.Equal(AccountsResultEnum.AccountsOk, result);
            _apiContaMock.Verify(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldTranferFundsBeNotAllowed()
        {
            var messageProcessorApp = new MessageProcessorApp(_apiContaMock.Object);
            _apiContaMock.Setup(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()))
                .ReturnsAsync(await Task.FromResult(new BalanceAdjustmentResponse() { Response = "Not enough balance" }));

            var result = await messageProcessorApp.TransferFunds(_transaction);

            Assert.Equal(AccountsResultEnum.NotAllowed, result);
            _apiContaMock.Verify(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()), Times.Once);
        }

        [Fact]
        public async Task ShouldTranferFundsBeNotReachable()
        {
            var messageProcessorApp = new MessageProcessorApp(_apiContaMock.Object);
            _apiContaMock.Setup(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()))
                .ReturnsAsync(await Task.FromResult(new BalanceAdjustmentResponse() { Response = "Not reachable" }));

            var result = await messageProcessorApp.TransferFunds(_transaction);

            Assert.Equal(AccountsResultEnum.NotReachable, result);
            _apiContaMock.Verify(x => x.PostTransferAsync(It.IsAny<BalanceAdjustment>()), Times.Once);
        }

    }
}
