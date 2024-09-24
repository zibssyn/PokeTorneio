public class Jogador
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public int Pontos { get; set; } = 0;
    public int Vitorias { get; set; } = 0;
    public int Derrotas { get; set; } = 0;
    public int Empates { get; set; } = 0;
    public Guid UsuarioId { get; set; }
    public bool TeveBye { get; set; }
    public List<string> ResultadosMelhorDe3 { get; set; } = new List<string>();

    // Nova propriedade para armazenar as partidas
    public ICollection<Partida> Partidas { get; set; } = new List<Partida>();

    public void RegistrarVitoria()
    {
        Vitorias++;
        Pontos += 3;
    }

    public void RegistrarEmpate()
    {
        Empates++;
        Pontos += 1;
    }

    public void RegistrarDerrota()
    {
        Derrotas++;
    }

    public void AdicionarResultadoMelhorDe3(string resultado)
    {
        ResultadosMelhorDe3.Add(resultado);
    }

    public int CalcularPontuacaoMelhorDe3()
    {
        int totalPontos = 0;

        foreach (var resultado in ResultadosMelhorDe3)
        {
            if (resultado == "2x0")
                totalPontos += 3;
            else if (resultado == "2x1")
                totalPontos += 2;
            else if (resultado == "1x0")
                totalPontos += 1;
            else totalPontos += 0;
        }

        return totalPontos;
    }
}
