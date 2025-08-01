using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplificacao_Normalizacao {
    public sealed record Execucao(NaoTerminal head, IReadOnlyList<Simbolo> body) {
        public bool ExecucaoUnitaria => body.Count == 1 && body[0] is NaoTerminal;
        public bool ExecucaoVazia => body.Count == 1 && body[0].Equals(Terminal.Vazio);

        public override string ToString() {
            return $"{head.texto} -> {string.Join(" ", body.Select(s => s.texto))}";
        }
    }
}

/*
 * Como seria em Java:
 * 
@Override
public String toString() {
return head.getTexto() + " -> " + body.stream()
.map(Simbolo::getTexto)
.collect(Collectors.joining(" "));
}
 */