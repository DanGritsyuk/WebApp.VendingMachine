﻿using Microsoft.EntityFrameworkCore;

namespace WebApp.VendingMachine
{
    public class VMDataBaseContext : DbContext
    {
        public VMDataBaseContext(DbContextOptions<VMDataBaseContext> options) : base(options) { }

        public DbSet<Coin> Coins { get; set; }
        public DbSet<Drink> Drinks { get; set; }
        public DbSet<VendingMachineViewModel> VendingMachineViewModel { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<VendingMachineViewModel>()
                .Property(p => p.Balance)
                .HasColumnType("decimal(18,2)");            
            
            builder.Entity<Drink>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<VendingMachineViewModel>().Ignore(x => x.Balance);
        }
    }
}