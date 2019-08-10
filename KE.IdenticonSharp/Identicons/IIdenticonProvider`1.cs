namespace IdenticonSharp.Identicons
{
    public interface IIdenticonProvider<TOptions> : IIdenticonProvider where TOptions : IIdenticonOptions
    {
        TOptions Options { get; }
    }
}
