namespace WebApplication1.Servises.KDF
{
    public interface IKdfServise
    {
        String GetDerivedKey(String password, String salt);
    }
}
