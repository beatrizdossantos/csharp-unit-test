using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
            mensagemErro = string.Empty;

            var ag = AgenciaRepository.GetById(agencia);

            if (ag == null)
            {
                mensagemErro = "Agência inválida!";
                return false;
            }

            var cc = ContaRepository.GetById(agencia, conta);

            if (cc == null)
            {
                mensagemErro = "Conta inválida!";
                return false;
            }

            if (valor <= 0)
            {
                mensagemErro = "O valor do depósito deve ser maior que zero!";
                return false;
            }

            cc.Saldo = cc.Saldo + valor;

            var extrato = new Extrato()
            {
                AgenciaId = agencia,
                ContaId = conta,
                Descricao = "Depósito",
                DataRegistro = DateTime.Now,
                Saldo = cc.Saldo,
                Valor = valor
            };

            try
            {
                using (var t = new TransactionScope())
                {
                    ContaRepository.Save(cc);
                    ExtratoRepository.Save(extrato);
                    t.Complete();
                }
            }
            catch (Exception)
            {
                mensagemErro = "Ocorreu um problema ao realizar o depósito!";
                return false;
            }

            return true;
        }

        public IList<Extrato> Extrato(int agencia, int conta, DateTime dataInicio, DateTime dataFim, out string mensagemErro)
        {
            mensagemErro = string.Empty;

            var ag = AgenciaRepository.GetById(agencia);

            if (ag == null)
            {
                mensagemErro = "Agência Inválida!";
                return null;
            }

            var cc = ContaRepository.GetById(agencia, conta);

            if (cc == null)
            {
                mensagemErro = "Conta Inválida!";
                return null;
            }

            if (dataInicio > dataFim)
            {
                mensagemErro = "A data de início deve ser menor que a data de fim!";
                return null;
            }

            if ((dataFim - dataInicio).Days > 120)
            {
                mensagemErro = "O período não deve ser superior a 120 dias!";
                return null;
            }

            try
            {
                var extrato = ExtratoRepository.GetByPeriodo(agencia, conta, dataInicio, dataFim);

                var linhaSaldo = new Extrato()
                {
                    Descricao = "Saldo Anterior",
                    Saldo = ExtratoRepository.GetSaldoAnterior(agencia, conta, dataInicio, dataFim)
                };

                extrato.Insert(0, linhaSaldo);

                return extrato;
            }
            catch (Exception ex)
            {
                mensagemErro = "Ocorreu um problema ao obter o extrato. Detalhes: " + ex.Message;
                return null;
            }
        }

        public decimal Saldo(int agencia, int conta, out string mensagemErro)
        {
             mensagemErro = string.Empty;

            var ag = AgenciaRepository.GetById(agencia);

            if (ag == null)
            {
                mensagemErro = "Agência Inválida!";
                return 0;
            }

            var cc = ContaRepository.GetById(agencia, conta);

            if (cc == null)
            {
                mensagemErro = "Conta Inválida!";
                return 0;
            }

            return cc.Saldo;
        }

        public bool Saque(int agencia, int conta, decimal valor, out string mensagemErro)
        {
            mensagemErro = string.Empty;

            var ag = AgenciaRepository.GetById(agencia);

            if (ag == null)
            {
                mensagemErro = "Agência Inválida!";
                return false;
            }

            var cc = ContaRepository.GetById(agencia, conta);

            if (cc == null)
            {
                mensagemErro = "Conta Inválida!";
                return false;
            }

            if (valor <= 0)
            {
                mensagemErro = "Valor do saque deve ser maior que zero!";
                return false;
            }

            if (valor > cc.Saldo)
            {
                mensagemErro = "Valor do saque ultrapassa o saldo!";
                return false;
            }

            cc.Saldo = cc.Saldo - valor;

            var extrato = new Extrato()
            {
                AgenciaId = agencia,
                ContaId = conta,
                Descricao = "Saque",
                DataRegistro = DateTime.Now,
                Saldo = cc.Saldo,
                Valor = valor * -1
            };

            try
            {
                using (var t = new TransactionScope())
                {
                    ContaRepository.Save(cc);
                    ExtratoRepository.Save(extrato);
                    t.Complete();
                }
            }
            catch (Exception ex)
            {
                mensagemErro = "Ocorreu um problema ao realizar o saque. Detalhes: " + ex.Message;
                return false;
            }

            return true;
        }

        public bool Transferencia(int agenciaOrigem, int contaOrigem, decimal valor, int agenciaPara, int contaPara, out string mensagemErro)
        {
            mensagemErro = string.Empty;

            var agOrigem = AgenciaRepository.GetById(agenciaOrigem);

            if (agOrigem == null)
            {
                mensagemErro = "Agência de origem Inválida!";
                return false;
            }

            var ccOrigem = ContaRepository.GetById(agenciaOrigem, contaOrigem);

            if (ccOrigem == null)
            {
                mensagemErro = "Conta de origem Inválida!";
                return false;
            }

            if (valor <= 0)
            {
                mensagemErro = "O valor da transferência deve ser maior que zero!";
                return false;
            }

            if (valor > ccOrigem.Saldo)
            {
                mensagemErro = "O valor da transferência deve ser menor ou igual ao saldo da conta de origem!";
                return false;
            }

            var agDestino = AgenciaRepository.GetById(agenciaPara);

            if (agDestino == null)
            {
                mensagemErro = "Agência de destino Inválida!";
                return false;
            }

            var ccDestino = ContaRepository.GetById(agenciaPara, contaPara);

            if (ccDestino == null)
            {
                mensagemErro = "Conta de destino Inválida!";
                return false;
            }

            ccOrigem.Saldo = ccOrigem.Saldo - valor;

            var extratoOrigem = new Extrato()
            {
                AgenciaId = agenciaOrigem,
                ContaId = contaOrigem,
                Descricao = $"Transferência para AG {agenciaPara} CC {contaPara}",
                DataRegistro = DateTime.Now,
                Saldo = ccOrigem.Saldo,
                Valor = valor * -1
            };

            ccDestino.Saldo = ccDestino.Saldo + valor;

            var extratoDestino = new Extrato()
            {
                AgenciaId = agenciaPara,
                ContaId = contaPara,
                Descricao = $"Transferência de AG {agenciaOrigem} CC {contaOrigem}",
                DataRegistro = DateTime.Now,
                Saldo = ccDestino.Saldo,
                Valor = valor
            };

            try
            {
                using (var t = new TransactionScope())
                {
                    ContaRepository.Save(ccOrigem);
                    ContaRepository.Save(ccDestino);

                    ExtratoRepository.Save(extratoOrigem);
                    ExtratoRepository.Save(extratoDestino);

                    t.Complete();
                }
            }
            catch (Exception ex)
            {
                mensagemErro = "Ocorreu um problema ao realizar a transferência. Detalhes: " + ex.Message;
                return false;
            }

            return true;
        }
    }
}
