using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.VendingMachine
{
    public class ImportDrinksViewModel
    {
        public ImportDrinksViewModel(List<Drink> drinks, string sessionId) 
        {
            Drinks = drinks;
            SessionId = sessionId;
        }

        public List<Drink> Drinks { get; set; } 
        public string SessionId { get; set; }
    }
}
