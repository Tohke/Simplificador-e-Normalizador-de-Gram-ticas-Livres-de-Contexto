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

        public void Simplificar() {
            RemoverProducoesVazias();
            RemoverProducoesUnitarias();
            RemoverSimbolosInuteis();
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
            for (int i = 1; i < numCombinacoes; i++) {
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
            yield return exec;
        }

        public void RemoverProducoesUnitarias() {
            bool alterou;
            do {
                alterou = false;
                var eUnitaria = Execucoes.FirstOrDefault(e => e.ExecucaoUnitaria);

                if (eUnitaria == null) break;

                alterou = true;
                Execucoes.Remove(eUnitaria);

                var A = eUnitaria.head;
                var B = (NaoTerminal)eUnitaria.body[0];

                if (A.Equals(B)) continue;

                var execucoesDeB = Execucoes.Where(e => e.head.Equals(B)).ToList();
                foreach (var eB in execucoesDeB) {
                    var novaExecucao = new Execucao(A, eB.body);
                    if (!Execucoes.Contains(novaExecucao)) {
                        Execucoes.Add(novaExecucao);
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
                foreach (var e in Execucoes.Where(e => alcancaveis.Contains(e.head)).ToList()) {
                    foreach (var s in e.body) alcancaveis.Add(s);
                }
            } while (alcancaveis.Count > tamanhoAnterior);

            Execucoes = Execucoes.Where(p => alcancaveis.Contains(p.head) && p.body.All(s => alcancaveis.Contains(s))).ToList();
            NaoTerminais.IntersectWith(alcancaveis.OfType<NaoTerminal>());
            Terminais.IntersectWith(alcancaveis.OfType<Terminal>());
        }

        public void ConverterParaChomsky() {
            var novasExecucoesTerm = new List<Execucao>();
            var mapTerminais = new Dictionary<Terminal, NaoTerminal>();

            foreach (var exec in Execucoes) {
                if (exec.body.Count > 1) {
                    var novoBody = new List<Simbolo>();
                    foreach (var simbolo in exec.body) {
                        if (simbolo is Terminal t) {
                            if (!mapTerminais.TryGetValue(t, out var ntTerminal)) {
                                ntTerminal = new NaoTerminal($"T_{t.texto}");
                                mapTerminais[t] = ntTerminal;
                                novasExecucoesTerm.Add(new Execucao(ntTerminal, new List<Simbolo> { t }));
                                NaoTerminais.Add(ntTerminal);
                            }
                            novoBody.Add(ntTerminal);
                        }
                        else {
                            novoBody.Add(simbolo);
                        }
                    }
                    novasExecucoesTerm.Add(new Execucao(exec.head, novoBody));
                }
                else {
                    novasExecucoesTerm.Add(exec);
                }
            }
            Execucoes = novasExecucoesTerm;

            var execucoesFinais = new List<Execucao>();
            var execucoesParaProcessar = new Queue<Execucao>(Execucoes);
            while (execucoesParaProcessar.Count > 0) {
                var exec = execucoesParaProcessar.Dequeue();
                if (exec.body.Count <= 2) {
                    execucoesFinais.Add(exec);
                }
                else {
                    var primeiroSimbolo = exec.body[0];
                    var restoDoBody = exec.body.Skip(1).ToList();
                    var novoNome = string.Join("", restoDoBody.Select(s => s.texto));
                    var novoNt = new NaoTerminal($"C_{novoNome}");

                    if (!NaoTerminais.Contains(novoNt)) {
                        NaoTerminais.Add(novoNt);
                        execucoesParaProcessar.Enqueue(new Execucao(novoNt, restoDoBody));
                    }
                    execucoesFinais.Add(new Execucao(exec.head, new List<Simbolo> { primeiroSimbolo, novoNt }));
                }
            }
            Execucoes = execucoesFinais;
        }

        public void RemoverRecursaoEsquerda() {
            var naoTerminaisProcessados = NaoTerminais.ToList();
            for (int i = 0; i < naoTerminaisProcessados.Count; i++) {
                var nt_i = naoTerminaisProcessados[i];
                for (int j = 0; j < i; j++) {
                    var nt_j = naoTerminaisProcessados[j];
                    var execucoesParaSubstituir = Execucoes
                        .Where(e => e.head.Equals(nt_i) && e.body.FirstOrDefault()?.Equals(nt_j) == true).ToList();

                    if (execucoesParaSubstituir.Any()) {
                        var execucoesAdicionadas = new List<Execucao>();
                        var execucoesDeJ = Execucoes.Where(e => e.head.Equals(nt_j)).ToList();
                        foreach (var exec in execucoesParaSubstituir) {
                            Execucoes.Remove(exec);
                            var gamma = exec.body.Skip(1).ToList();
                            foreach (var exec_j in execucoesDeJ) {
                                var novoBody = new List<Simbolo>(exec_j.body);
                                novoBody.AddRange(gamma);
                                execucoesAdicionadas.Add(new Execucao(nt_i, novoBody));
                            }
                        }
                        Execucoes.AddRange(execucoesAdicionadas);
                    }
                }
                EliminarRecursaoImediata(nt_i);
            }
        }

        private void EliminarRecursaoImediata(NaoTerminal nt_A) {
            var execucoes = Execucoes.Where(e => e.head.Equals(nt_A)).ToList();
            var execucoesRecursivas = execucoes.Where(e => e.body.FirstOrDefault()?.Equals(nt_A) == true).ToList();
            var execucoesNaoRecursivas = execucoes.Except(execucoesRecursivas).ToList();

            if (!execucoesRecursivas.Any()) return;

            var novoNt = new NaoTerminal(nt_A.texto + "'");
            NaoTerminais.Add(novoNt);
            Execucoes.RemoveAll(e => e.head.Equals(nt_A));

            foreach (var beta in execucoesNaoRecursivas) {
                var novoBody = new List<Simbolo>(beta.body);
                novoBody.Add(novoNt);
                Execucoes.Add(new Execucao(nt_A, novoBody));
            }
            foreach (var alpha in execucoesRecursivas) {
                var novoBody = new List<Simbolo>(alpha.body.Skip(1));
                novoBody.Add(novoNt);
                Execucoes.Add(new Execucao(novoNt, novoBody));
            }
            Execucoes.Add(new Execucao(novoNt, new List<Simbolo> { Terminal.Vazio }));
        }

        // SUBSTITUA APENAS ESTE MÉTODO EM Gramatica.cs

        public void FatorarEsquerda() {
            bool foiAlterado;
            do {
                foiAlterado = false;
                NaoTerminal? ntParaFatorar = null;
                IGrouping<Simbolo?, Execucao>? grupoParaFatorar = null;

                // Encontra o primeiro não-terminal que precisa de fatoração
                foreach (var nt in NaoTerminais) {
                    grupoParaFatorar = Execucoes
                        .Where(e => e.head.Equals(nt))
                        .GroupBy(e => e.body.FirstOrDefault())
                        .FirstOrDefault(g => g.Count() > 1 && g.Key != null);

                    if (grupoParaFatorar != null) {
                        ntParaFatorar = nt;
                        break;
                    }
                }

                // Se encontrou algo para fatorar, realiza a transformação
                if (ntParaFatorar != null && grupoParaFatorar != null) {
                    foiAlterado = true; // Marca que a gramática mudou para que o laço continue
                    var execucoesDoGrupo = grupoParaFatorar.ToList();
                    var prefixoComum = grupoParaFatorar.Key!;

                    // Cria um nome único para o novo não-terminal (ex: A', A'', etc.)
                    var novoNt = new NaoTerminal(ntParaFatorar.texto + "'");
                    while (NaoTerminais.Contains(novoNt)) {
                        novoNt = new NaoTerminal(novoNt.texto + "'");
                    }
                    NaoTerminais.Add(novoNt);

                    // 1. CORREÇÃO: Remove TODAS as produções antigas que serão fatoradas
                    Execucoes.RemoveAll(e => execucoesDoGrupo.Contains(e));

                    // 2. Adiciona a nova produção principal fatorada: A -> αA'
                    Execucoes.Add(new Execucao(ntParaFatorar, new List<Simbolo> { prefixoComum, novoNt }));

                    // 3. Adiciona as novas produções para A'
                    foreach (var exec in execucoesDoGrupo) {
                        var sufixo = exec.body.Skip(1).ToList();

                        // 4. CORREÇÃO: Se o sufixo for vazio, a nova produção é para épsilon (Vazio)
                        var corpoNovoNt = sufixo.Any() ? sufixo : new List<Simbolo> { Terminal.Vazio };
                        var novaExecucao = new Execucao(novoNt, corpoNovoNt);

                        // Evita adicionar produções duplicadas para o novo não-terminal
                        if (!Execucoes.Contains(novaExecucao)) {
                            Execucoes.Add(novaExecucao);
                        }
                    }
                }
            } while (foiAlterado);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"Símbolo Inicial: {SimboloInicial.texto}");
            sb.AppendLine("Execuções:");
            var execucoesAgrupadas = Execucoes.GroupBy(e => e.head).OrderBy(g => g.Key.texto);
            foreach (var grupo in execucoesAgrupadas) {
                var head = grupo.Key.texto;
                var bodys = grupo.Select(p => string.Join(" ", p.body.Select(s => s.texto)));
                sb.AppendLine($"  {head} -> {string.Join(" | ", bodys)}");
            }
            return sb.ToString();
        }
    }
}