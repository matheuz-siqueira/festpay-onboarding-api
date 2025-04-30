using System.Text.RegularExpressions;

namespace Festpay.Onboarding.Domain.Extensions;

public static class ValidationExtension
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return mail.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhone(this string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var regex = new Regex(@"^\(?\d{11}\)?$");
        return regex.IsMatch(phone);
    }

    public static bool IsValidDocument(this string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var digitsOnly = new string(document.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length == 11)
            return IsValidCpf(digitsOnly);

        if (digitsOnly.Length == 14)
            return IsValidCnpj(digitsOnly);

        return false;
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1)
            return false;

        var numbers = cpf.Select(x => int.Parse(x.ToString())).ToArray();

        int sum1 = 0;
        for (int i = 0; i < 9; i++)
            sum1 += numbers[i] * (10 - i);

        int digit1 = sum1 % 11;
        digit1 = digit1 < 2 ? 0 : 11 - digit1;

        int sum2 = 0;
        for (int i = 0; i < 10; i++)
            sum2 += numbers[i] * (11 - i);

        int digit2 = sum2 % 11;
        digit2 = digit2 < 2 ? 0 : 11 - digit2;

        return numbers[9] == digit1 && numbers[10] == digit2;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1)
            return false;

        var numbers = cnpj.Select(x => int.Parse(x.ToString())).ToArray();

        int[] weight1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weight2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int sum1 = 0;
        for (int i = 0; i < 12; i++)
            sum1 += numbers[i] * weight1[i];

        int digit1 = sum1 % 11;
        digit1 = digit1 < 2 ? 0 : 11 - digit1;

        int sum2 = 0;
        for (int i = 0; i < 13; i++)
            sum2 += numbers[i] * weight2[i];

        int digit2 = sum2 % 11;
        digit2 = digit2 < 2 ? 0 : 11 - digit2;

        return numbers[12] == digit1 && numbers[13] == digit2;
    }
}
