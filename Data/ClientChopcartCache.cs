using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace WebApp.VendingMachine
{
    [DataContract]
    public class ClientChopcartCache
    {
        public ClientChopcartCache() { }

        public ClientChopcartCache(Guid shopcartId) 
        {
            ShopcartId = shopcartId;
            DrinksToClient = null;
            ClientMoney = 0;
        }

        [DataMember(Name = "ChopcartId")]
        public Guid ShopcartId { get; set;  }

        [DataMember(Name = "DrinksToClient")]
        public List<Drink> DrinksToClient { get; set; }

        [DataMember(Name = "ClientMoney")]
        public decimal ClientMoney { get; set; }

        [IgnoreDataMember]
        public decimal OrderSum => DrinksToClient != null ? DrinksToClient.Sum(dr => dr.Price * dr.Count) : 0;
    }
}
