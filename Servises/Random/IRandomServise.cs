namespace WebApplication1.Servises.Random
{
    public interface IRandomServise
    {
        String ConfirmCode(int lenght);
        String RandomString(int length);
        String RandomAvatarName(String fileName, int length);
    }
}
