namespace PokeTorneio.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int IdPokemon { get; set; }

        public List<Jogador> Jogadores { get; set; } = new List<Jogador>();
    }

}
