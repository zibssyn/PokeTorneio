namespace PokeTorneio.Models
{
    public class Torneio
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataInicio { get; set; }
        public TimeSpan Duracao { get; set; }
        public bool IsFinalizado { get; set; }

        public List<Jogador> Jogadores { get; set; } = new List<Jogador>();
        public List<Rodada> Rodadas { get; set; } = new List<Rodada>();

        public void AdicionarJogador(Jogador jogador)
        {
            Jogadores.Add(jogador);
        }

        public void AdicionarRodada(Rodada rodada)
        {
            Rodadas.Add(rodada);
        }
    }
}
