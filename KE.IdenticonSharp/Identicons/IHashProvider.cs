namespace IdenticonSharp.Identicons
{
    public interface IHashProvider
    {
        byte[] ComputeHash(byte[] input);
    }
}
