namespace WebApplication1.Servises.Hash
{
    public interface IHashServise
    {
        /// <summary>
        /// Обчислює хеш від рядкового аргументу та подає його у гексадецимальному вигляді
        /// </summary>
        /// <param name="text">Вхідний текст</param>
        /// <returns>Гексадецимальний рядок з хеш-образом тексту</returns>
        String Hash(String text);
    }
}
