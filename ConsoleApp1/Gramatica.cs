using Simplificacao_Normalizacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplificacao_Normalizacao {
    public class Gramatica {
        public ISet<NaoTerminal> NaoTerminais { get; private set; }
        public ISet<Terminal> Terminais { get; private set; }
        public NaoTerminal SimboloInicial { get; private set; }
        public List<Execucao> Execucoes { get; private set; }

        public Gramatica(ISet<NaoTerminal> naoTerminais, ISet<Terminal> terminais, NaoTerminal simboloInicial, List<Execucao> execucoes) {
            NaoTerminais = naoTerminais;
            Terminais = terminais;
            SimboloInicial = simboloInicial;
            Execucoes = execucoes;
        }

        public static Gramatica FromString(string input) {
            var naoTerminais = new HashSet<NaoTerminal>();
            var terminais = new HashSet<Terminal>();
            var execucoes = new List<Execucao>();
            NaoTerminal simboloInicial = null!;

            var linhas = input.Trim().Split('\n');

            foreach (var linha in linhas) {
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var partes = linha.Split("->");
                var headStr = partes[0].Trim();
                var head = new NaoTerminal(headStr);
                naoTerminais.Add(head);

                if (simboloInicial is null) {
                    simboloInicial = head;
                }

                var bodyStrings = partes[1].Trim().Split('|');
                foreach (var bodyStr in bodyStrings) {
                    var body = new List<Simbolo>();
                    var simbolosStr = bodyStr.Trim().Split(' ');
                    foreach (var s in simbolosStr) {
                        if (string.IsNullOrEmpty(s)) continue;

                        if (char.IsUpper(s[0])) {
                            var nt = new NaoTerminal(s);
                            body.Add(nt);
                            naoTerminais.Add(nt);
                        }
                        else {
                            var t = s == "ε" ? Terminal.Vazio : new Terminal(s);
                            body.Add(t);
                            if (t != Terminal.Vazio) terminais.Add(t);
                        }
                    }
                    execucoes.Add(new Execucao(head, body));
                }
            }
            return new Gramatica(naoTerminais, terminais, simboloInicial, execucoes);
        }

        public void RemoverProducoesVazias() {
            var anulaveis = Execucoes
                .Where(e => e.ExecucaoVazia)
                .Select(e => e.head)
                .ToHashSet();

            int tamanhoAnterior;
            do {
                tamanhoAnterior = anulaveis.Count;
                foreach (var e in Execucoes) {
                    if (e.body.All(s => s is NaoTerminal nt && anulaveis.Contains(nt))) {
                        anulaveis.Add(e.head);
                    }
                }
            } while (anulaveis.Count > tamanhoAnterior);

            if (anulaveis.Count == 0) return;

            var novasExecucoes = new List<Execucao>();
            foreach (var e in Execucoes) {
                novasExecucoes.AddRange(GerarCombinacoesSemVazio(e, anulaveis));
            }

            Execucoes = novasExecucoes.Where(e => !e.ExecucaoVazia).Distinct().ToList();

            if (anulaveis.Contains(SimboloInicial)) {
                var novoSimboloInicial = new NaoTerminal(SimboloInicial.texto + "'");
                NaoTerminais.Add(novoSimboloInicial);

                Execucoes.Insert(0, new Execucao(novoSimboloInicial, new List<Simbolo> { SimboloInicial }));
                Execucoes.Insert(1, new Execucao(novoSimboloInicial, new List<Simbolo> { Terminal.Vazio }));

                SimboloInicial = novoSimboloInicial;
            }
        }

        private IEnumerable<Execucao> GerarCombinacoesSemVazio(Execucao exec, ISet<NaoTerminal> anulaveis) {
            var indicesAnulaveis = exec.body
                .Select((simbolo, index) => new { simbolo, index })
                .Where(x => x.simbolo is NaoTerminal nt && anulaveis.Contains(nt))
                .Select(x => x.index)
                .ToList();

            if (indicesAnulaveis.Count == 0) {
                yield return exec;
                yield break;
            }

            int numCombinacoes = 1 << indicesAnulaveis.Count;
            for (int i = 0; i < numCombinacoes; i++) {
                var novoBody = new List<Simbolo>();
                var indicesAPular = new HashSet<int>();
                for (int j = 0; j < indicesAnulaveis.Count; j++) {
                    if ((i & (1 << j)) > 0) {
                        indicesAPular.Add(indicesAnulaveis[j]);
                    }
                }

                for (int k = 0; k < exec.body.Count; k++) {
                    if (!indicesAPular.Contains(k)) {
                        novoBody.Add(exec.body[k]);
                    }
                }

                if (novoBody.Count > 0) {
                    yield return new Execucao(exec.head, novoBody);
                }
            }
        }

        public void RemoverProducoesUnitarias() {
            bool alterou;
            do {
                alterou = false;
                var eUnitaria = Execucoes.FirstOrDefault(e => e.ExecucaoUnitaria);

                if (eUnitaria == null) break;

                Execucoes.Remove(eUnitaria);

                var A = eUnitaria.head;
                var B = (NaoTerminal)eUnitaria.body[0];

                foreach (var eB in Execucoes.Where(e => e.head.Equals(B))) {
                    var novaExecucao = new Execucao(A, eB.body);
                    if (!Execucoes.Contains(novaExecucao)) {
                        Execucoes.Add(novaExecucao);
                        alterou = true;
                    }
                }
            } while (alterou);
        }

        public void RemoverSimbolosInuteis() {
            var produtivos = new HashSet<NaoTerminal>();
            int tamanhoAnterior;
            do {
                tamanhoAnterior = produtivos.Count;
                foreach (var e in Execucoes) {
                    if (e.body.All(s => s is Terminal || (s is NaoTerminal nt && produtivos.Contains(nt)))) {
                        produtivos.Add(e.head);
                    }
                }
            } while (produtivos.Count > tamanhoAnterior);

            Execucoes = Execucoes
                .Where(e => produtivos.Contains(e.head) && e.body.All(s => s is Terminal || produtivos.Contains(s)))
                .ToList();

            NaoTerminais.IntersectWith(produtivos);

            var alcancaveis = new HashSet<Simbolo> { SimboloInicial };
            do {
                tamanhoAnterior = alcancaveis.Count;
                foreach (var e in Execucoes.Where(e => alcancaveis.Contains(e.head))) {
                    foreach (var s in e.body) alcancaveis.Add(s);
                }
            } while (alcancaveis.Count > tamanhoAnterior);

            Execucoes = Execucoes.Where(p => alcancaveis.Contains(p.head)).ToList();
            foreach (var nt in NaoTerminais.Where(nt => !alcancaveis.Contains(nt)).ToList()) {
                NaoTerminais.Remove(nt);
            }
            
            foreach (var nt in Terminais.Where(nt => !alcancaveis.Contains(nt)).ToList()) {
                Terminais.Remove(nt);
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"Símbolo Inicial: {SimboloInicial.texto}");
            sb.AppendLine("Execuções:");

            var execucoesAgrupadas = Execucoes.GroupBy(e => e.head);

            foreach (var grupo in execucoesAgrupadas) {
                var head = grupo.Key.texto;
                var bodys = grupo.Select(p => string.Join(" ", p.body.Select(s => s.texto)));
                sb.AppendLine($"  {head} -> {string.Join(" | ", bodys)}");
            }

            return sb.ToString();
        }
    }
}