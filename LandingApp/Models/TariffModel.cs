    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace LandingApp.Models
    {
        public class TariffModel
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; } = null!;
            [StringLength(500)]
            public string Description { get; set; }
            public int Speed { get; set; }
            [StringLength(100)]
            public string ConnectionOperator { get; set; }
            public bool IsContract { get; set; }
            public bool IsTv { get; set; }

            [Range(0, 5)]
            public int ContractDuration { get; set; }

            public int SimcardNum { get; set; }

            [Range(0, int.MaxValue)]
            public int PricePerMonth { get; set; }


        }
    }
