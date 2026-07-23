using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PaymentWebhookLogConfiguration : IEntityTypeConfiguration<PaymentWebhookLog>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookLog> builder)
    {
        builder.ToTable("PAYMENT_WEBHOOK_LOGS");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("WebhookLogId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Provider)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.OrderCode)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(x => x.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.ProcessResult)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.HasIndex(x => x.OrderCode);
        builder.HasIndex(x => x.ReceivedAt);
    }
}
