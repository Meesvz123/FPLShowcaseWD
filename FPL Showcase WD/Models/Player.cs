namespace FPL_Showcase_WD.Models
{
    public class Player
    {
        public int Id { get; set; }

        public string Naam { get; set; } = string.Empty;

        public string Positie { get; set; } = string.Empty;

        public string Club { get; set; } = string.Empty;

        public int Prijs { get; set; }

        public int Statistieken { get; set; }
    }
}
