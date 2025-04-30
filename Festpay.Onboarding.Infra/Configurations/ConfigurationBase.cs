using Festpay.Onboarding.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Festpay.Onboarding.Infra.Configurations;

public abstract class ConfigurationBase<T> where T : EntityBase
{
    protected void ConfigureEntityBase(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Id).ValueGeneratedOnAdd().IsRequired();

        builder.Property(k => k.CreatedUtc).ValueGeneratedOnAdd();

        builder.Property(k => k.DeactivatedUtc);
    }
}