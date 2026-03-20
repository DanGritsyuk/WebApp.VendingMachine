using System.Runtime.Serialization;

namespace VendingMachine.Common.Entities.VendingMachineModels
{
    public class ClientCartCache
    {
        public ClientCartCache()
        {
            CartId = Guid.NewGuid();
            Items = new();
            InsertedCoins = new();
            ClientMoney = 0m;
        }

        [DataMember(Name = "CartId")]
        public Guid CartId { get; set; }

        [DataMember(Name = "Items")]
        public List<Drink> Items { get; set; }

        [DataMember(Name = "ClientMoney")]
        public decimal ClientMoney { get; set; }

        [DataMember(Name = "InsertedCoins")]
        public List<Coin> InsertedCoins { get; set; }

        public decimal OrderSum =>
            Items.Count == 0 ? 0m : Items.Sum(item => item.Price * item.Count);
    }
}
