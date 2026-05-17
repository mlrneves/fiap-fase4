namespace Infrastructure.CrossCutting.Correlation
{
    public interface ICorrelationIdGenerator
    {
        string Get();
    }
}