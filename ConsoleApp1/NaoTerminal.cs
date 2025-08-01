using Simplificacao_Normalizacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplificacao_Normalizacao {
    public sealed record NaoTerminal(string texto) : Simbolo(texto);
}








/*namespace Simplificacao_Normalizacao {
    internal class NaoTerminal : Simbolo{
        public NaoTerminal(string texto) : base(texto) {}
    }
}*/