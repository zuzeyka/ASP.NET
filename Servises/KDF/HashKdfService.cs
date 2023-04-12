using WebApplication1.Servises.Hash;

namespace WebApplication1.Servises.KDF
{
    public class HashKdfService : IKdfServise
    {
        private readonly IHashServise _hashServise;

        public HashKdfService(IHashServise hashServise)
        {
            _hashServise = hashServise;
        }

        public String GetDerivedKey(String password, String salt) 
        {
            return _hashServise.Hash(salt + password);
        }
    }
}
