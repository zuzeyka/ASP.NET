namespace WebApplication1.Models.Home.User
{
    public class RegisterValidationModel
    {
        public String LoginMessage { get; set; } = null!;
        public String PasswordMessage { get; set; } = null!;
        public String RepeatPasswordMessage { get; set; } = null!;
        public String EmailMessage { get; set; } = null!;
        public String RealNameMessage { get; set; } = null!;
    }
}
