using System;

namespace WebApp.VendingMachine
{
    public class ShopcartViewModel
    {
        public ShopcartViewModel() { }

        public ShopcartViewModel(Tuple<Guid, string, decimal, decimal, VendingMachineViewModel> parameters)
        {
            ShopcartId = parameters.Item1;
            SessionId = parameters.Item2;
            ClientMoney = parameters.Item3;
            OrderSum = parameters.Item4;
            ThisVM = parameters.Item5;
    }

        public Guid ShopcartId { get; set; }
        public string SessionId { get; set; }        
        public decimal ClientMoney { get; set; }
        public decimal OrderSum { get; set; }

        public VendingMachineViewModel ThisVM { get; set; }
    }
}
