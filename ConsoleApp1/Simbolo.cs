using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Simplificacao_Normalizacao {
    public abstract record Simbolo(string texto);
}


/*namespace Simplificacao_Normalizacao {
    internal record class Simbolo { //O mesmo que abstract, porém abstract é mutável, já record é imutável
        protected String texto { get; init; }

        //Construtor
        public Simbolo(String texto) {
            if (String.IsNullOrWhiteSpace(texto)) {
                throw new ArgumentNullException("Palavra vazia");
            }
            this.texto = texto;
        }
        //Override
        public String getTexto() {
            return texto;
        }
    }*/