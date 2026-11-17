using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models;

public class Selection
{
    public int Id { get; set; }

    [Required]
    public int RequestId { get; set; }

    [Required]
    public int PropertyId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsViewed { get; set; }

    [MaxLength(1000)]
    public string? ClientComment { get; set; }

    public Request Request { get; set; } = null!;

    public Property Property { get; set; } = null!;
}