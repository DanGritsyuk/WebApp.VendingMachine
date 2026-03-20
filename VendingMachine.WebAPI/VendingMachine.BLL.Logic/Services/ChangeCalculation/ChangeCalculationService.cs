using VendingMachine.BLL.Logic.Contracts.Services;
using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.BLL.Logic.Services.ChangeCalculation
{
    public class ChangeCalculationService : IChangeCalculationService
    {
        public IReadOnlyCollection<Coin> CalculateChange(decimal changeAmount, IEnumerable<Coin> availableCoins)
        {
            if (availableCoins == null) throw new ArgumentNullException(nameof(availableCoins));

            var roundedChange = (int)Math.Round(changeAmount, MidpointRounding.AwayFromZero);
            if (roundedChange < 0) throw new ArgumentOutOfRangeException(nameof(changeAmount));
            if (roundedChange == 0) return Array.Empty<Coin>();

            var workingCoins = availableCoins
                .Where(c => c.IsAvailable && c.Count > 0)
                .Select(c => new WorkingCoin(c.ItemId, c.Denomination, c.Count))
                .ToList();

            if (workingCoins.Count == 0)
            {
                throw new InvalidOperationException("No coins available for change.");
            }

            var totalBalance = workingCoins.Sum(c => c.GetDenominationValue() * c.Count);
            if (roundedChange > totalBalance)
            {
                throw new InvalidOperationException("Insufficient coins in machine.");
            }

            var selected = new Dictionary<int, WorkingCoin>();
            var remaining = roundedChange;

            while (remaining > 0)
            {
                SortCoinsForIssue(workingCoins);
                var issued = false;

                foreach (var coin in workingCoins)
                {
                    var denomination = coin.GetDenominationValue();
                    if (coin.Count == 0 || denomination > remaining)
                    {
                        continue;
                    }

                    coin.Count--;
                    remaining -= denomination;
                    issued = true;

                    if (!selected.TryGetValue(coin.ItemId, out var selectedCoin))
                    {
                        selected[coin.ItemId] = new WorkingCoin(coin.ItemId, coin.Denomination, 1);
                    }
                    else
                    {
                        selectedCoin.Count++;
                    }

                    break;
                }

                if (!issued)
                {
                    throw new InvalidOperationException("Unable to calculate exact change with available coins.");
                }
            }

            return selected.Values
                .OrderByDescending(c => c.GetDenominationValue())
                .Select(c => new Coin(c.ItemId, c.Denomination, c.Count, isAvailable: true))
                .ToArray();
        }

        private static void SortCoinsForIssue(List<WorkingCoin> coins)
        {
            var startPoint = coins.Min(c => c.GetDenominationValue() * c.Count);

            coins.Sort((left, right) =>
            {
                var leftWeight = CalculateWeight(left.GetDenominationValue() * left.Count, startPoint);
                var rightWeight = CalculateWeight(right.GetDenominationValue() * right.Count, startPoint);

                var byWeight = rightWeight.CompareTo(leftWeight);
                if (byWeight != 0)
                {
                    return byWeight;
                }

                return right.GetDenominationValue().CompareTo(left.GetDenominationValue());
            });
        }

        private static double CalculateWeight(int coinSum, int startPoint)
        {
            var delta = coinSum - startPoint;
            return delta > 0 ? Math.Sqrt(delta) : 0d;
        }
    }
}
