using Microsoft.EntityFrameworkCore;
using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.DAL.Repository
{
    public class VendingMachineDbContext : DbContext
    {
        public VendingMachineDbContext(DbContextOptions<VendingMachineDbContext> options) : base(options) { }

        public DbSet<Coin> Coins { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Drink> Drinks { get; set; }

        /// <summary>
        /// Конфигурирует модель при создании контекста базы данных.
        /// </summary>
        /// <param name="modelBuilder">Строитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureCoin(modelBuilder);
            ConfigureBrand(modelBuilder);
            ConfigureDrink(modelBuilder);
        }

        /// <summary>
        /// Конфигурирует сущность монет.
        /// </summary>
        /// <param name="modelBuilder">Строитель модели.</param>
        private void ConfigureCoin(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coin>(entity =>
            {
                entity.HasKey(e => new { e.ItemId })
                      .HasName("PK_Coins");

                entity.Property(e => e.ItemId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Denomination)
                      .IsRequired();

                entity.Property(e => e.IsAvailable)
                      .IsRequired();

                entity.Property(e => e.Count)
                      .IsRequired()
                      .HasColumnName("CountInVM");

                entity.ToTable("CoinInventory");
            });
        }

        /// <summary>
        /// Конфигурирует сущность бренда.
        /// </summary>
        /// <param name="modelBuilder">Строитель модели.</param>
        private void ConfigureBrand(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.BrandId)
                      .HasName("PK_Brands");

                entity.Property(e => e.BrandId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.HasIndex(e => e.Name)
                      .IsUnique();

                entity.ToTable("Brands");
            });
        }

        /// <summary>
        /// Конфигурирует сущность товара.
        /// </summary>
        /// <param name="modelBuilder">Строитель модели.</param>
        private void ConfigureDrink(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Drink>(entity =>
            {
                entity.HasKey(e => e.ItemId)
                      .HasName("PK_Drinks");

                entity.Property(e => e.ItemId)
                      .HasColumnOrder(1)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Price)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Count)
                      .IsRequired();

                entity.Property(e => e.IsAvailable)
                      .IsRequired();

                entity.Property(e => e.ImageUrl)
                      .IsRequired();

                entity.HasOne(d => d.Brand)
                      .WithMany(b => b.Drinks)
                      .HasForeignKey(d => d.BrandId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Drinks_Brands");

                entity.Property(e => e.BrandId)
                      .IsRequired();

                entity.ToTable("Drinks");
            });
        }
    }
}
