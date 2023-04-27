namespace WebApplication1.Servises.Validation
{
    public interface IValidationService
    {
        bool Validate(String source, params ValidationTerms[] terms);
    }
}
