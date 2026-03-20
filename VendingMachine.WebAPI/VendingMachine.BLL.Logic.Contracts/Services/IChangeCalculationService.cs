using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.BLL.Logic.Contracts.Services
{
    public interface IChangeCalculationService
    {
        IReadOnlyCollection<Coin> CalculateChange(decimal changeAmount, IEnumerable<Coin> availableCoins);
    }
}
