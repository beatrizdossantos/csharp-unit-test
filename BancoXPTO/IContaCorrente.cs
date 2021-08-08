﻿using System;
using System.Collections.Generic;

namespace BancoXPTO
{
    public interface IContaCorrente
    {
        bool Deposito(int agencia, int conta, decimal valor, out string mensagemErro);

        bool Saque(int agencia, int conta, decimal valor, out string mensagemErro);

        decimal Saldo(int agencia, int conta, out string mensagemErro);

        bool Transferencia(int agenciaOrigem, int contaOrigem, decimal valor, int agenciaPara, int contaPara,  out string mensagemErro);

        IList<Extrato> Extrato(int agencia, int conta, DateTime dataInicio, DateTime dataFim, out string mensagemErro);
    }
}