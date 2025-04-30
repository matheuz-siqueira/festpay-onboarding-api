namespace Festpay.Onboarding.Domain.Entities;

public class EntityBase
{
    public Guid Id { get; }
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;
    public DateTime? DeactivatedUtc { get; private set; }

    public virtual void Validate() { }

    public virtual void EnableDisable()
    {
        if (DeactivatedUtc.HasValue)
            DeactivatedUtc = null;
        else
            DeactivatedUtc = DateTime.UtcNow;
    }
}
