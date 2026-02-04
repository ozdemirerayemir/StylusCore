using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StylusCore.Core.Models;

namespace StylusCore.Core.Services
{
    /// <summary>
    /// Service for validating data integrity rules.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates if a library name is acceptable and unique.
        /// </summary>
        ValidationResult ValidateLibraryName(string name, IEnumerable<Library> existingLibraries);

        /// <summary>
        /// Validates if a notebook name is acceptable and unique within its library.
        /// </summary>
        ValidationResult ValidateNotebookName(string name, Guid libraryId, IEnumerable<Notebook> existingNotebooks);
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true);
        public static ValidationResult Fail(string message) => new ValidationResult(false, message);
    }

    public class ValidationService : IValidationService
    {
        private const int MaxNameLength = 255;
        private static readonly char[] InvalidPathChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray();

        public ValidationResult ValidateLibraryName(string name, IEnumerable<Library> existingLibraries)
        {
            // 1. Empty Check
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Fail("Library name cannot be empty.");
            }

            name = name.Trim();

            // 2. Length Check
            if (name.Length > MaxNameLength)
            {
                return ValidationResult.Fail($"Library name cannot be longer than {MaxNameLength} characters.");
            }

            // 3. Invalid Characters Check
            if (name.Any(c => InvalidPathChars.Contains(c)))
            {
                return ValidationResult.Fail("Library name contains invalid characters (\\ / : * ? \" < > |).");
            }

            // 4. Global Uniqueness Check
            if (existingLibraries.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Fail($"A library with the name '{name}' already exists.");
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateNotebookName(string name, Guid libraryId, IEnumerable<Notebook> existingNotebooks)
        {
            // 1. Empty Check
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidationResult.Fail("Notebook name cannot be empty.");
            }

            name = name.Trim();

            // 2. Length Check
            if (name.Length > MaxNameLength)
            {
                return ValidationResult.Fail($"Notebook name cannot be longer than {MaxNameLength} characters.");
            }

            // 3. Invalid Characters Check
            if (name.Any(c => InvalidPathChars.Contains(c)))
            {
                return ValidationResult.Fail("Notebook name contains invalid characters (\\ / : * ? \" < > |).");
            }

            // 4. Scoped Uniqueness Check (Only check notebooks in the SAME library)
            // Note: existingNotebooks should ideally be ALL notebooks, filtering is done here.
            // Or if existingNotebooks passed is pre-filtered, the logic still holds (LibraryId check becomes redundant but safe).
            if (existingNotebooks.Any(n => n.LibraryId == libraryId && n.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return ValidationResult.Fail($"A notebook with the name '{name}' already exists in this library.");
            }

            return ValidationResult.Success();
        }
    }
}
