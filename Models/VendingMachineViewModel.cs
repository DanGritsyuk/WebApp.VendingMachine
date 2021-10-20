using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApp.VendingMachine
{
    public class VendingMachineViewModel
    {
        [Key]
        [Column(Order = 1)]
        public Guid ItemId { get; set; }

        [Required]
        public string Name { get; set; }

        public List<Drink> Drinks { get; set; }
        public List<Coin> Coins { get; set; }

        [NotMapped]
        public decimal Balance =>
            Coins.Where(cn => cn.vendingMachine.ItemId == ItemId).Sum(cn => cn.Denomination * cn.CountInVM);


        public static VendingMachineViewModel GetDataFromBase(VendingMachineContext context, Guid thisMachineGuid)
        {
            try
            {
                var vendingMachineViewModel = context.VendingMachineViewModel.Where(vm => vm.ItemId == thisMachineGuid).FirstOrDefault();

                if (vendingMachineViewModel != null)
                {
                    vendingMachineViewModel.Drinks = context.Drinks.Where(dr => dr.vendingMachine == vendingMachineViewModel).ToList();
                    vendingMachineViewModel.Coins = context.Coins.Where(cn => cn.vendingMachine == vendingMachineViewModel).ToList();
                }
                return vendingMachineViewModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}