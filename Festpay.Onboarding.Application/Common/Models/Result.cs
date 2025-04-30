using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Festpay.Onboarding.Application.Common.Models;

public class Result<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class Result
{
    private Result(object? data, bool success, string? message)
    {
        Data = data;
        Success = success;
        Message = message;
    }

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    public static IResult Ok(object? data, string? message = null)
    {
        return Results.Ok(new Result(data, true, message));
    }

    public static IResult Created(object? data = null, string? message = null)
    {
        return Results.Created("", new Result(data, true, message));
    }

    public static Result Failure(string message)
    {
        return new Result(null, false, message);
    }
}
