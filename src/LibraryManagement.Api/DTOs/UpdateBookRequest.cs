using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs;

public sealed class UpdateBookRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1000, 9999)]
    public int PublishedYear { get; set; }

    [Required]
    public bool? IsAvailable { get; set; }
}
