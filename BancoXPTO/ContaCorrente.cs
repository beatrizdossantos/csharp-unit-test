using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BancoXPTO
{
    public class ContaCorrente : IContaCorrente
    {
        public IAgenciaRepository AgenciaRepository { get; set; }

        public IContaRepository ContaRepository { get; set; }
        public IExtratoRepository ExtratoRepository { get; set; }


        public ContaCorrente(IAgenciaRepository agenciaRepository, IContaRepository contaRepository, IExtratoRepository extratoRepository)
        {
            AgenciaRepository = agenciaRepository;
            ContaRepository = contaRepository;
            ExtratoRepository = extratoRepository;
        }

        public bool Deposito(int agencia, int conta, decimal valor, out string mensagemErro)
        {
            throw new NotImplementedException();
        }

        public IList<Extrato> Extrato(int agencia, int conta, DateTime dataInicio, DateTime dataFim, out string mensagemErro)
        {
            throw new NotImplementedException();
        }

        public decimal Saldo(int agencia, int conta, out string mensagemErro)
        {
            throw new NotImplementedException();
        }

        public bool Saque(int agencia, int conta, decimal valor, out string mensagemErro)
        {
            throw new NotImplementedException();
        }

        public bool Transferencia(int agenciaOrigem, int contaOrigem, decimal valor, int agenciaPara, int contaPara, out string mensagemErro)
        {
            throw new NotImplementedException();
        }
    }
}
