namespace WebApplication1.Servises.Email
{
    public interface IEmailService
    {
        bool Send(String mailTemplate, object model);
    }
}
