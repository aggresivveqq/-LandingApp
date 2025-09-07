namespace LandingApp.Dto
{
    public class LeadDto
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? City { get; set; }
        public string TariffName { get; set; } = null!;
        public string? Comment { get; set; }
        public string? Source { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
