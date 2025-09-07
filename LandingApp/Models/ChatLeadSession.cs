namespace LandingApp.Models
{
    public class ChatLeadSession
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? TariffName { get; set; }
        public string? Need { get; set; }
        public bool AwaitingConfirmation { get; set; } = false;

     
        public bool IsCompleted =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Phone) &&
            !string.IsNullOrWhiteSpace(Address) &&
            !string.IsNullOrWhiteSpace(City) &&
            !string.IsNullOrWhiteSpace(TariffName) &&
            !string.IsNullOrWhiteSpace(Need); 
    }
}