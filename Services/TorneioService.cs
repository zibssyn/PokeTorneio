using Microsoft.EntityFrameworkCore;
using PokeTorneio.Data;
using PokeTorneio.Enums;
using PokeTorneio.Models;
using System.Collections.Generic;
using System.Linq;

namespace PokeTorneio.Services
{
    public class TorneioService : ITorneioService
    {
        private readonly ApplicationDbContext _context;

        public TorneioService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listagem de Torneios
        public IEnumerable<Torneio> ListarTorneios() => _context.Torneios.ToList();

        public int CalcularNumeroDeRodadas(int numeroDeJogadores)
        {
            return numeroDeJogadores < 2 ? 0 : (int)Math.Ceiling(Math.Log(numeroDeJogadores, 2));
        }

        // Obtenção de Torneios
        public Torneio ObterTorneioPorId(int torneioId) =>
            _context.Torneios
                .Include(t => t.Jogadores)
                .Include(t => t.Rodadas)
                    .ThenInclude(r => r.Partidas)
                .FirstOrDefault(t => t.Id == torneioId);

        public Torneio ObterTorneioPorPartida(int partidaId) =>
            _context.Torneios
                .FirstOrDefault(t => t.Rodadas.Any(r => r.Partidas.Any(p => p.Id == partidaId)));

        // Adição de Torneios e Jogadores
        public void AdicionarTorneio(Torneio torneio)
        {
            _context.Torneios.Add(torneio);
            _context.SaveChanges();
        }

        public void AdicionarJogadores(int torneioId, IEnumerable<Jogador> jogadores)
        {
            // Obter o torneio a partir do ID
            var torneio = _context.Torneios
                .Include(t => t.Jogadores) // Inclui a lista de jogadores para evitar problemas de referência
                .FirstOrDefault(t => t.Id == torneioId);

            if (torneio != null)
            {
                // Adiciona os jogadores ao torneio
                foreach (var jogador in jogadores)
                {
                    torneio.Jogadores.Add(jogador);
                }
                _context.SaveChanges(); // Salva as mudanças no contexto
            }
        }


        // Finalização do Torneio
        public void FinalizarTorneio(int torneioId)
        {
            var torneio = ObterTorneioPorId(torneioId);
            if (torneio != null)
            {
                torneio.IsFinalizado = true;
                _context.SaveChanges();
            }
        }

        // Lógica de Vencedor
        public Jogador CalcularVencedor(Torneio torneio)
        {
            var jogadoresComVitorias = torneio.Jogadores
                .Where(j => j.Vitorias > 0)
                .OrderByDescending(j => j.Vitorias)
                .ThenByDescending(j => j.CalcularPontuacaoMelhorDe3())
                .ToList();

            if (jogadoresComVitorias.Count > 1 && jogadoresComVitorias[0].Vitorias == jogadoresComVitorias[1].Vitorias)
            {
                return CompararDesempenho(jogadoresComVitorias[0], jogadoresComVitorias[1]);
            }

            return jogadoresComVitorias.FirstOrDefault();
        }

        private Jogador CompararDesempenho(Jogador j1, Jogador j2)
        {
            return CalcularPontuacao(j1) > CalcularPontuacao(j2) ? j1 : j2;
        }

        private int CalcularPontuacao(Jogador jogador) =>
            jogador.Partidas.Sum(p => CalcularResultadoPartida(p));

        public void SalvarPartida(Partida partida)
        {
            var partidaExistente = _context.Partidas.Find(partida.Id);

            if (partidaExistente != null)
            {
                partidaExistente.Resultado = partida.Resultado;
                partidaExistente.ResultadoMelhorDe3 = partida.ResultadoMelhorDe3;
                AtualizarResultadoJogadores(partidaExistente, partida.Resultado);
                _context.Partidas.Update(partidaExistente);
            }
            else
            {
                partida.Jogador1.Partidas.Add(partida);
                partida.Jogador2.Partidas.Add(partida);
                _context.Partidas.Add(partida);
            }

            _context.SaveChanges();
        }

        private int CalcularResultadoPartida(Partida partida) =>
            partida.ResultadoMelhorDe3 switch
            {
                Enums.ResultadoMelhorDe3.DoisAZero => 3,
                Enums.ResultadoMelhorDe3.DoisAUm => 2,
                Enums.ResultadoMelhorDe3.UmAZero => 1,
                _ => 0,
            };

        public void RegistrarResultado(int partidaId, ResultadoMelhorDe3 resultadoMelhorDe3, int resultado, Guid vencedorId)
        {
            Partida partida = _context.Partidas.Find(partidaId);
            if (partida == null) return;

            partida.Resultado = resultado;
            partida.ResultadoMelhorDe3 = resultadoMelhorDe3;

            if (resultado == 0) // Empate
            {
                partida.Jogador1.RegistrarEmpate();
                partida.Jogador2.RegistrarEmpate();
                partida.VencedorId = null;
            }
            else if (resultado == 1) // Jogador 1 vence
            {
                partida.Jogador1.AdicionarResultadoMelhorDe3(EnumHelper.GetDescription(resultadoMelhorDe3));
                partida.Jogador1.RegistrarVitoria();
                partida.Jogador1.Pontos = partida.Jogador1.Pontos + partida.Jogador1.CalcularPontuacaoMelhorDe3();

                partida.Jogador2.RegistrarDerrota();
            }
            else if (resultado == 2) // Jogador 2 vence
            {
                partida.Jogador2.AdicionarResultadoMelhorDe3(EnumHelper.GetDescription(resultadoMelhorDe3));
                partida.Jogador2.RegistrarVitoria();
                partida.Jogador2.Pontos = partida.Jogador2.Pontos + partida.Jogador2.CalcularPontuacaoMelhorDe3();

                partida.Jogador1.RegistrarDerrota();
            }

            if (resultado != 0)
            {
                partida.VencedorId = vencedorId;
            }
            _context.SaveChanges();
        }



        // Método para iniciar a rodada
        public Rodada IniciarRodada(int torneioId)
        {
            var torneio = ObterTorneioPorId(torneioId);
            if (torneio == null || !torneio.Jogadores.Any()) return null;

            var rodada = new Rodada
            {
                NumeroRodada = torneio.Rodadas.Count + 1,
                TorneioId = torneioId
            };

            // Verifica quantas rodadas devem ser criadas com base no número de jogadores
            int numRodadas = CalcularNumeroDeRodadas(torneio.Jogadores.Count);
            if (rodada.NumeroRodada > numRodadas) return null; // Não cria mais rodadas do que o necessário

            CriarPartidas(rodada, torneio);
            torneio.AdicionarRodada(rodada);
            _context.SaveChanges();
            return rodada;
        }
        private void CriarPartidas(Rodada rodada, Torneio torneio)
        {
            var jogadores = torneio.Jogadores.ToList(); // Captura todos os jogadores
            var partidas = new List<Partida>();
            var jogadoresBye = new List<Jogador>();

            // Verifica se é a primeira rodada
            if (rodada.NumeroRodada == 1)
            {
                // Criar partidas para a primeira rodada
                for (int i = 0; i < jogadores.Count; i += 2) // Incrementa de 2 em 2 para emparelhar
                {
                    if (i + 1 < jogadores.Count) // Verifica se há um próximo jogador
                    {
                        var partida = new Partida
                        {
                            Jogador1Id = jogadores[i].Id,
                            Jogador2Id = jogadores[i + 1].Id,
                            RodadaId = rodada.Id,
                            TorneioId = rodada.TorneioId,
                            Resultado = null,
                            ResultadoMelhorDe3 = null,
                            VencedorId = null
                        };

                        partidas.Add(partida);
                    }
                    else
                    {
                        // Se o número de jogadores é ímpar, registra o jogador como bye
                        jogadoresBye.Add(jogadores[i]);
                    }
                }

                // Gerenciar bye
                if (jogadoresBye.Count > 0)
                {
                    // Escolhe aleatoriamente o jogador que receberá o bye na primeira rodada
                    var jogadorBye = jogadoresBye[new Random().Next(jogadoresBye.Count)];
                    jogadorBye.RegistrarVitoria();
                    jogadorBye.TeveBye = true;
                }

                // Adiciona todas as partidas à rodada
                rodada.Partidas.AddRange(partidas);
                return; // Sai da função após tratar a primeira rodada
            }

            // Lógica para rodadas subsequentes

            var jogadoresEmparelhados = new HashSet<Guid>();

            var partidasAnteriores = _context.Partidas.Where(t => t.TorneioId == torneio.Id).ToList();

            // Ordena jogadores por "teve bye" e, em seguida, por pontos
            jogadores = jogadores.OrderByDescending(j => j.TeveBye).ThenByDescending(j => j.Pontos).ToList();

            // Emparelhamento de jogadores
            for (int i = 0; i < jogadores.Count; i++)
            {
                var jogador1 = jogadores[i];

                if (jogadoresEmparelhados.Contains(jogador1.Id))
                    continue;

                bool emparelhado = false;

                for (int j = i + 1; j < jogadores.Count; j++) // Começa do próximo jogador
                {
                    var jogador2 = jogadores[j];

                    // Ignora se já emparelhou ou se é o mesmo jogador
                    if (jogadoresEmparelhados.Contains(jogador2.Id) || jogador1.Id == jogador2.Id) continue;

                    // Verifica se já se enfrentaram
                    if (!JaSeEnfrentaram(jogador1, jogador2, partidasAnteriores))
                    {
                        var partida = new Partida
                        {
                            Jogador1Id = jogador1.Id,
                            Jogador2Id = jogador2.Id,
                            RodadaId = rodada.Id,
                            TorneioId = torneio.Id,
                            Resultado = null,
                            ResultadoMelhorDe3 = null,
                            VencedorId = null
                        };

                        partidas.Add(partida);
                        jogadoresEmparelhados.Add(jogador1.Id);
                        jogadoresEmparelhados.Add(jogador2.Id);
                        emparelhado = true;
                        break; // Sair do loop após emparelhamento
                    }
                }

                // Se não encontrou adversário e o total de jogadores é ímpar
                if (!emparelhado && jogadores.Count % 2 != 0)
                {
                    jogadoresBye.Add(jogador1); // Adiciona à lista de bye
                }
            }

            // Gerenciar bye
            if (jogadoresBye.Count > 0)
            {
                var jogadorBye = jogadoresBye.First(); // Escolhe o jogador que ficou de bye
                jogadorBye.RegistrarVitoria(); // Registrar a vitória do jogador que recebeu bye
                jogadorBye.TeveBye = true; // Marcar que o jogador teve bye
            }

            // Adiciona as partidas à rodada
            rodada.Partidas.AddRange(partidas);


        }

        // Método para verificar se dois jogadores já se enfrentaram
        private bool JaSeEnfrentaram(Jogador jogador1, Jogador jogador2, List<Partida> partidas)
        {
            return partidas.Any(p => (p.Jogador1Id == jogador1.Id && p.Jogador2Id == jogador2.Id) ||
                                      (p.Jogador1Id == jogador2.Id && p.Jogador2Id == jogador1.Id));
        }





        // Método para verificar se os jogadores já se enfrentaram
        public bool JogadoresJaSeEnfrentaram(Guid jogador1Id, Guid jogador2Id) =>
            _context.Partidas.Any(p =>
                (p.Jogador1Id == jogador1Id && p.Jogador2Id == jogador2Id) ||
                (p.Jogador1Id == jogador2Id && p.Jogador2Id == jogador1Id));

        // Método para obter uma rodada por ID
        public Rodada ObterPorRodadaId(int rodadaId) =>
            _context.Rodadas
                .Include(r => r.Partidas)
                    .ThenInclude(p => p.Jogador1)
                .Include(r => r.Partidas)
                    .ThenInclude(p => p.Jogador2)
                .FirstOrDefault(r => r.Id == rodadaId);

        public Partida ObterPartidaPorId(int partidaId) =>
            _context.Partidas
                .Include(p => p.Jogador1)
                .Include(p => p.Jogador2)
                .FirstOrDefault(p => p.Id == partidaId);

        public void AtualizarResultadoJogadores(Partida partida, int? resultado)
        {
            AtualizarJogadores(partida, resultado);
            _context.SaveChanges();
        }

        private void AtualizarJogadores(Partida partida, int? resultado)
        {
            switch (resultado)
            {
                case 1:
                    partida.Jogador1.RegistrarVitoria();
                    partida.Jogador2.RegistrarDerrota();
                    break;
                case 2:
                    partida.Jogador2.RegistrarVitoria();
                    partida.Jogador1.RegistrarDerrota();
                    break;
                case 0:
                    partida.Jogador1.RegistrarEmpate();
                    partida.Jogador2.RegistrarEmpate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultado), "Resultado inválido.");
            }
        }
    }
}