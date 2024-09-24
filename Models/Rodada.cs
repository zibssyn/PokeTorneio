namespace PokeTorneio.Models
{
    public class Rodada
    {
        public int Id { get; set; }
        public int NumeroRodada { get; set; }
        public int TorneioId { get; set; }
        public Torneio Torneio { get; set; }
        public List<Partida> Partidas { get; set; } = new List<Partida>();
    }

}
