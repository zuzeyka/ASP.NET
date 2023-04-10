namespace WebApplication1.Servises
{
    public class TimeServise
    {
        public DateTime GetMoment() { return DateTime.Now.ToLocalTime(); }
    }
}
