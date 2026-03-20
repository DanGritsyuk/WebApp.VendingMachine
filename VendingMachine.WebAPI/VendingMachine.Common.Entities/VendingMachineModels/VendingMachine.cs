namespace VendingMachine.Common.Entities.VendingMachineModels
{
    public class VendingMachine
    {
        public VendingMachine()
        {
            Drinks = new List<Drink>();
            Coins = new List<Coin>();
            IsAvailable = false;
        }

        public List<Drink> Drinks { get; set; }
        public List<Coin> Coins { get; set; }

        public bool IsAvailable { get; set; }
    }
}
