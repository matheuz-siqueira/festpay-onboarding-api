using System.Dynamic;

namespace Festpay.Onboarding.Domain.Exceptions;

public class DomainException : ArgumentException
{
    public string? FieldName { get; }

    protected DomainException(string message)
        : base(message)
    {
        FieldName = string.Empty;
    }

    protected DomainException(string value, string fieldName)
        : base($"{fieldName}: {value}")
    {
        FieldName = fieldName;
    }
}

public class InvalidEmailFormatException(string email)
    : DomainException($"The email '{email}' is not in a valid format.", nameof(email))
{
    public string Email { get; } = email;
}

public class InvalidPhoneNumberException(string phone)
    : DomainException($"The phone number '{phone}' is not valid.", nameof(phone))
{
    public string Phone { get; } = phone;
}

public class InvalidDocumentNumberException(string document)
    : DomainException($"The document number '{document}' is not valid.", nameof(document))
{
    public string Document { get; } = document;
}

public class RequiredFieldException(string fieldName)
    : DomainException($"The field '{fieldName}' is required and cannot be empty.", fieldName);

public class InvalidEnumValueException(string enumName, string value)
    : DomainException($"The value '{value}' is not valid for the enum '{enumName}'.", enumName)
{
    public string EnumName { get; } = enumName;
    public string Value { get; } = value;
}

public class InvalidDateRangeException(DateTime startDate, DateTime endDate)
    : DomainException(
        $"The end date '{endDate}' cannot be earlier than the start date '{startDate}'.",
        nameof(endDate)
    )
{
    public DateTime StartDate { get; } = startDate;
    public DateTime EndDate { get; } = endDate;
}

public class InvalidHourlyRateException(decimal hourlyRate)
    : DomainException(
        $"The hourly rate '{hourlyRate}' must be greater than zero.",
        nameof(hourlyRate)
    )
{
    public decimal HourlyRate { get; } = hourlyRate;
}

public class InsufficientBalanceException()
    : DomainException($"The account does not have sufficient funds for the transaction.");

public class EqualAccountsException(Guid destinationAccountId)
    : DomainException($"The destination account must be different from the source account.", nameof(destinationAccountId));

public class InvalidAmountException(decimal amount) :
    DomainException($"The transaction amount '{amount}' must be greater than zero.", nameof(amount))
{
    public decimal Amount { get; } = amount;
}

public class TransactionAlreadyCanceledException(Guid transactionId) :
    DomainException($"Transaction '{transactionId}' is already canceled.", nameof(transactionId))
{
    public Guid TransactionId { get; } = transactionId;
}
