using PokeTorneio.Enums;
using PokeTorneio.Models;

public class Partida
{
    public int Id { get; set; }
    public Jogador Jogador1 { get; set; }
    public Guid Jogador1Id { get; set; }
    public Jogador Jogador2 { get; set; }
    public Guid? Jogador2Id { get; set; }
    public ResultadoMelhorDe3? ResultadoMelhorDe3 { get; set; }
    public int? Resultado { get; set; } 
    public Guid? VencedorId { get; set; } 
    public int TorneioId { get; set; }
    public Torneio Torneio { get; set; }
    public int RodadaId { get; set; }
    public Rodada Rodada { get; set; }
}
