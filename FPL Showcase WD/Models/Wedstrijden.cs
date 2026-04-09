namespace FPL_Showcase_WD.Models
{
    public class Wedstrijden
    {
        public int Id { get; set; }
        public string Thuisclub { get; set; }
        public int UitClub { get; set; }

        public DateTime Datum { get; set; }

        public string Uitslag { get; set; }

        public string Statistieken { get; set; }


    }
}
