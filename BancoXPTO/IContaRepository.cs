namespace BancoXPTO
{
    public interface IContaRepository
    {
        Conta GetById(int agenciaId, int contaId);

        void Save(Conta conta);

    }
}