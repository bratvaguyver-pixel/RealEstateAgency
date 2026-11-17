using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models;

public class PropertyPhoto
{
    public int Id { get; set; }

    [Required]
    public int PropertyId { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public bool IsMain { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Property Property { get; set; } = null!;
}