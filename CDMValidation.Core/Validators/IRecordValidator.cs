using CDMValidation.Core.Models;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Interface for record validators.
/// </summary>
/// <typeparam name="T">The record type to validate.</typeparam>
public interface IRecordValidator<T>
{
    /// <summary>
    /// Validates a record and returns a list of validation errors.
    /// </summary>
    List<ValidationError> Validate(T record);
}
