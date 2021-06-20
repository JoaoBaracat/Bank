using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Infra.Data.Mappings
{
    public class TransactionMapping : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AccountOrigin).IsRequired()
                .HasMaxLength(20)
                .HasColumnType("VARCHAR(20)");

            builder.Property(x => x.AccountDestination).IsRequired()
                .HasMaxLength(20)
                .HasColumnType("VARCHAR(20)");

            builder.Property(x => x.Value).IsRequired();

            builder.ToTable("Transactions");

        }
    }
}
