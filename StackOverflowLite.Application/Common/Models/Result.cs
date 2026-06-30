namespace StackOverflowLite.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static Result<T> Failure(string error) => new() { Succeeded = false, Error = error };
}

public class Result
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}
