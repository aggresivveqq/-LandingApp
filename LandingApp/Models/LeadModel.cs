using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace LandingApp.Models
{
    public class LeadModel
    {
            [Key]
            public int Id { get; set; }

            [Required, StringLength(300)]
            public string Name { get; set; } = null!;

            [Required, Phone]
            public string Phone { get; set; } = null!;

           [StringLength(100)]
            public string? City { get; set; }
            public string TariffName { get; set; } = null!; 
            public string? Comment { get; set; } 
            public string? Need { get; set; }

            public string? Source { get; set; }

            public bool IsExported { get; set; } = false;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
