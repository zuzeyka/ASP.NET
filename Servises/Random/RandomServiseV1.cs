﻿using System.Text;
using WebApplication1.Models.Home.User;

namespace WebApplication1.Servises.Random
{
    public class RandomServiseV1 : IRandomServise
    {
        private readonly String chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        private readonly String safeChars = new String(
            Enumerable.Range(20, 107).Select(x => (char)x).ToArray());
        private readonly System.Random random = new();
        
        public String ConfirmCode(int length)
        {
            
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
        }
        public String RandomString(int length) 
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(safeChars.Length);
                sb.Append(safeChars[index]);
            }
            return sb.ToString();
        }

        public String RandomAvatarName(string fileName, int length)
        {
            String ext = Path.GetExtension(fileName);
            return ConfirmCode(length) + ext;
        }
    }
}
