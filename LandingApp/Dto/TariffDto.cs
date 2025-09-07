using System.ComponentModel.DataAnnotations;

namespace LandingApp.Dto
{
    public class TariffDto
    {
       
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; }
        public int Speed { get; set; }
        public string ConnectionOperator { get; set; }
        public bool IsContract { get; set; }
        public bool IsTv { get; set; }

        public int ContractDuration { get; set; }

        public int SimcardNum { get; set; }

        public int PricePerMonth { get; set; }


    }
}
