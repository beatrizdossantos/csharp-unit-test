using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using FizzWare.NBuilder;
using System.Linq;

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

            var agencia = new Agencia() { Id = 100, Nome = "Agência Teste" };
            var agencia2 = new Agencia() { Id = 100, Nome = "Agência Teste 2" };

            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(100)).Returns(agencia);
            Mock.Get(cc.AgenciaRepository).Setup(r => r.GetById(200)).Returns(agencia2);

            var conta = new Conta() { Id = 555, AgenciaId = 100, NomeCliente = "Xuxa", CPFCliente = "12345679844", Saldo = 100m };
            var conta2 = new Conta() { Id = 700, AgenciaId = 200, NomeCliente = "Faustão", CPFCliente = "10987654321", Saldo = 200m };

            Mock.Get(cc.ContaRepository).Setup(r => r.GetById(agencia.Id, conta.Id)).Returns(conta);
            Mock.Get(cc.ContaRepository).Setup(r => r.GetById(agencia2.Id, conta2.Id)).Returns(conta2);

            return cc;
        }

        [TestMethod]
        public void Deposito_erro_se_agencia_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

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

            // Act
            string error;
            var result = cc.Deposito(100, 1234, 100m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Conta inválida!", error);
        }

        [TestMethod]
        public void Deposito_erro_se_valor_menor_ou_igual_zero()
        {
            // Arrange
            var cc = GetContaCorrente();

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

        [TestMethod]
        public void Saque_erro_se_conta_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saque(100, 000, 50m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Conta Inválida", error);
        }

        public void Saque_erro_se_valor_menor_ou_igual_que_zero()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saque(100, 555, -1m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Valor do saque deve ser maior que zero!", error);
        }

        public void Saque_erro_se_valor_maior_que_saldo_conta()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saque(100, 555, 110m, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Valor do saque ultrapassa o saldo!", error);
        }

        public void Saque_retorna_true_se_realizado_com_sucesso()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saque(100, 555, 50m, out error);

            // Assert
            Assert.IsTrue(result);
            Mock.Get(cc.ContaRepository).Verify(r => r.Save(It.Is<Conta>(c => c.AgenciaId == 100 && c.Id == 555 && c.Saldo == 50m)));
            Mock.Get(cc.ExtratoRepository).Verify(r => r.Save(It.Is<Extrato>(e => e.AgenciaId == 100 && e.ContaId == 555 && e.Descricao == "Saque" && e.DataRegistro.Date == DateTime.Today && e.Valor == -50m && e.Saldo == 50m)));
        }

        [TestMethod]
        public void Transferencia_retorna_true_se_realizado_com_sucesso()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 555, 50m, 200, 700, out error);

            // Assert
            Assert.IsTrue(result);

            // conta origem
            Mock.Get(cc.ContaRepository).Verify(r => r.Save(It.Is<Conta>(c => c.AgenciaId == 100 && c.Id == 555 && c.Saldo == 50m)));
            Mock.Get(cc.ExtratoRepository).Verify(r => r.Save(It.Is<Extrato>(e => e.AgenciaId == 100 && e.ContaId == 555 && e.Descricao == "Transferência para AG 200 CC 700" && e.DataRegistro.Date == DateTime.Today && e.Valor == -50m && e.Saldo == 50m)));

            // conta destino
            Mock.Get(cc.ContaRepository).Verify(r => r.Save(It.Is<Conta>(c => c.AgenciaId == 200 && c.Id == 700 && c.Saldo == 250m)));
            Mock.Get(cc.ExtratoRepository).Verify(r => r.Save(It.Is<Extrato>(e => e.AgenciaId == 200 && e.ContaId == 700 && e.Descricao == "Transferência de AG 100 CC 555" && e.DataRegistro.Date == DateTime.Today && e.Valor == 50m && e.Saldo == 250m)));
        }

        [TestMethod]
        public void Transferencia_erro_se_agencia_origem_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(000, 555, 50m, 200, 700, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Agência de origem Inválida", error);
        }

        [TestMethod]
        public void Transferencia_erro_se_agencia_destino_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 555, 100m, 000, 444, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Agência de destino Inválida", error);
        }

        public void Transferencia_erro_se_valor_menor_ou_igual_a_zero()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 555, 0m, 200, 700, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("O valor da transnferência deve ser maior que zero!", error);
        }

        public void Transferencia_erro_se_valor_maior_que_saldo_origem()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 555, 200m, 200, 700, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("O valor da transnferência deve ser menor ou igual ao saldo da conta de origem!", error);
        }

        [TestMethod]
        public void Transferencia_erro_se_conta_origem_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 000, 50m, 200, 700, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Conta de origem Inválida", error);
        }

        [TestMethod]
        public void Transferencia_erro_se_conta_destino_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Transferencia(100, 555, 100m, 200, 000, out error);

            // Assert
            Assert.IsFalse(result);
            Assert.Equals("Conta de destino Inválida", error);
        }

        [TestMethod]
        public void Saldo_retorna_saldo_da_conta()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saldo(100, 555, out error);

            // Assert
            Assert.AreEqual(100m, result);
        }

        [TestMethod]
        public void Saldo_erro_se_agencia_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saldo(000, 555, out error);

            // Assert
            Assert.AreEqual(0m, result);
            Assert.Equals("Agência Inválida", error);
        }

        [TestMethod]
        public void Saldo_erro_se_conta_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Saldo(100, 000, out error);

            // Assert
            Assert.AreEqual(0m, result);
            Assert.Equals("Conta Inválida", error);
        }

        [TestMethod]
        public void Extrato_retorna_registros_do_extrato()
        {
            // Arrange
            var cc = GetContaCorrente();
            var dataInicio = new DateTime(2021, 01, 01);
            var dataFim = new DateTime(2020, 01, 15);


            var extrato = Builder<Extrato>.CreateListOfSize(10).All()
                                                                .With(e => e.AgenciaId = 100)
                                                                .With(e => e.ContaId = 555).Build();

            Mock.Get(cc.ExtratoRepository).Setup(r => r.GetByPeriodo(100, 555, dataInicio, dataFim)).Returns(extrato);

            // Act
            string error;
            var result = cc.Extrato(100, 555, dataInicio, dataFim, out error);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(11, result.Count);
            Assert.AreEqual(extrato.Sum(e => e.Valor), result.Sum(r => r.Valor));
        }

        [TestMethod]
        public void Extrato_primeira_linha_contem_saldo_anterior()
        {
            // Arrange
            var cc = GetContaCorrente();
            var dataInicio = new DateTime(2021, 01, 01);
            var dataFim = new DateTime(2020, 01, 15);


            var extrato = Builder<Extrato>.CreateListOfSize(10).All()
                                                                .With(e => e.AgenciaId = 100)
                                                                .With(e => e.ContaId = 555).Build();

            Mock.Get(cc.ExtratoRepository).Setup(r => r.GetByPeriodo(100, 555, dataInicio, dataFim)).Returns(extrato);
            Mock.Get(cc.ExtratoRepository).Setup(r => r.GetSaldoAnterior(100, 555, dataInicio, dataFim)).Returns(30m);

            // Act
            string error;
            var result = cc.Extrato(100, 555, dataInicio, dataFim, out error);

            // Assert
            Assert.AreEqual("Saldo Anterior", result.First().Descricao);
            Assert.AreEqual(30m, result.First().Saldo);
        }

        [TestMethod]
        public void Extrato_erro_se_data_inicio_maior_data_fim()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Extrato(100, 555, new DateTime(2021, 01, 01), new DateTime(2020, 01, 15), out error);

            // Assert
            Assert.IsNull(result);
            Assert.Equals("A data de início deve ser menor que a data de fim!", error);
        }

        [TestMethod]
        public void Extrato_erro_se_periodo_maior_120_dias()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Extrato(100, 555, new DateTime(2020, 01, 01), new DateTime(2020, 01, 15).AddDays(121), out error);

            // Assert
            Assert.IsNull(result);
            Assert.Equals("O período não deve ser superior a 120 dias!", error);
        }

        [TestMethod]
        public void Extrato_erro_se_agencia_nao_existir()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Extrato(000, 555, new DateTime(2020, 01, 01), new DateTime(2020, 01, 15), out error);

            // Assert
            Assert.IsNull(result);
            Assert.Equals("Agência Inválida", error);
        }

        [TestMethod]
        public void Extrato_erro_se_conta_nao_existir_na_agencia()
        {
            // Arrange
            var cc = GetContaCorrente();

            // Act
            string error;
            var result = cc.Extrato(100, 000, new DateTime(2020, 01, 01), new DateTime(2020, 01, 15), out error);

            // Assert
            Assert.IsNull(result);
            Assert.Equals("Conta Inválida", error);
        }
    }
}
