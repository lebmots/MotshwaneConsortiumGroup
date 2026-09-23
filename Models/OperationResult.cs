namespace MotshwaneConsortiumGroup.Models;

/// <summary>
/// Wraps the outcome of a business operation so services can report a validation
/// failure (e.g. "unit not available") without throwing an exception for it.
/// Controllers check Success and show Error to the user when it's false.
/// </summary>
public class OperationResult<T>
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public T? Value { get; private set; }

    public static OperationResult<T> Ok(T value) => new() { Success = true, Value = value };
    public static OperationResult<T> Fail(string error) => new() { Success = false, Error = error };
}
