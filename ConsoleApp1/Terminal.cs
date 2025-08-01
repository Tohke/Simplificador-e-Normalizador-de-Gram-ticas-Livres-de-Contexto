using Simplificacao_Normalizacao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Simplificacao_Normalizacao {
    public sealed record Terminal(string texto) : Simbolo(texto) {
        // Em C#, "static readonly" é o equivalente ao "public static final" do Java.
        public static readonly Terminal Vazio = new("ε");
    }
}
/*namespace Simplificacao_Normalizacao {
    internal class Terminal : Simbolo { //extends
        public static sealed Terminal VAZIO = new Terminal("ε");
        public Terminal(string texto) : base(texto) {}//base() = super()
    }
}*/

//esse contrutor passa o texto para o construtor da classe mãe, basicamente um super() invertido
//public Variavel(string texto