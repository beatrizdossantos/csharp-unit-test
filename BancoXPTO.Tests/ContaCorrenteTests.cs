using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace BancoXPTO.Tests
{
    [TestClass]
    public class ContaCorrenteTests
    {
        private ContaCorrente GetContaCorrente()
        {
            var cc = new ContaCorrente(
                Mock.Of<IAgenciaRepository>(),
                Mock.Of<IContaRepository>(),
                Mock.Of<IExtratoRepository>()
            );

            return cc;
        }

        [TestMethod]
        public void Deposito_erro_se_agencia_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();
            var agencia = new Agencia() { Id = 100, Nome = "Agência Teste" };

            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(100)).Returns(agencia);

            // Act
            string error;
            var result = cc.Deposito(000, 1234, 100m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Agência inválida!", error);
        }

        [TestMethod]
        public void Deposito_erro_se_conta_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();
            var agencia = new Agencia() { Id = 100, Nome = "Agência Teste" };

            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(100)).Returns(agencia);

            // Act
            string error;
            var result = cc.Deposito(000, 1234, 100m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Conta inválida!", error);
        }

        [TestMethod]
        public void Deposito_erro_se_valor_menor_ou_igual_zero()
        {
            // Arrange
            var cc = GetContaCorrente();
            var agencia = new Agencia() { Id = 100, Nome = "Agência Teste" };
            var conta = new Conta() { Id = 555, AgenciaId = 100, NomeCliente = "Xuxa", CPFCliente = "12345679844", Saldo = 100};

            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(100)).Returns(agencia);
            Mock.Get(cc.ContaRepository).Setup(r => r.GetById(agencia.Id, conta.Id)).Returns(conta);

            // Act
            string error;
            var result = cc.Deposito(100, 555, 0m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("O valor do depósito deve ser maior que zero!", error);
        }

        [TestMethod]
        public void Deposito_retorna_true_se_realizado_com_sucesso()
        {
            // Arrange
            var cc = GetContaCorrente();
            var agencia = new Agencia() { Id = 100, Nome = "Agência Teste" };
            var conta = new Conta() { Id = 555, AgenciaId = 100, NomeCliente = "Xuxa", CPFCliente = "12345679844", Saldo = 100 };

            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(100)).Returns(agencia);
            Mock.Get(cc.ContaRepository).Setup(r => r.GetById(agencia.Id, conta.Id)).Returns(conta);

            // Act
            string error;
            var result = cc.Deposito(100, 555, 50m, out error);

            // Assert
            Assert.IsTrue(result);
            Mock.Get(cc.ContaRepository).Verify(r => r.Save(It.Is<Conta>(c => c.AgenciaId == 100 && c.Id == 555 && c.Saldo == 150m)));
            Mock.Get(cc.ExtratoRepository).Verify(r => r.Save(It.Is<Extrato>(e => e.AgenciaId == 100 && e.ContaId == 555 && e.Descricao == "Depósito" && e.DataRegistro.Date == DateTime.Today && e.Valor == 50m && e.Saldo == 150m)));
        }

        [TestMethod]
        public void Saque_erro_se_agencia_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saque(000, 555, 50m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Agência Inválida", error);
        }
    }
}
