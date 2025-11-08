using System.ComponentModel.DataAnnotations;

namespace Azure_File_Ingestion.Dto
{
    public sealed class FileUploadDto : IValidatableObject
    {
        [Required]
        public IFormFile File { get; init; } = default!;

        [Required, MaxLength(64, ErrorMessage = "Name max 64 chars.")]
        [RegularExpression(@"^[A-Za-z0-9 _.\-]+$", ErrorMessage = "Name contains invalid characters.")]
        public string Name { get; init; } = string.Empty;

        [Required]
        [RegularExpression(@"^\+?[1-9]\d{6,14}$", ErrorMessage = "Invalid phone.")]
        public string Contact { get; init; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (File is null)
            {
                yield return new ValidationResult("File is required.", new[] { nameof(File) });
                yield break;
            }
            if (File.Length == 0)
                yield return new ValidationResult("Empty file.", new[] { nameof(File) });

            if (!string.Equals(File.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                yield return new ValidationResult("Only PDF files are allowed.", new[] { nameof(File) });

            if (!string.Equals(Path.GetExtension(File.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                yield return new ValidationResult("Only .pdf extension is allowed.", new[] { nameof(File) });
        }
    }
}