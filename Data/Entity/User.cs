﻿using System.Text;

namespace WebApplication1.Data.Entity
{
    public class User
    {
        public Guid Id              { get; set; }
        public String Login         { get; set; } = null!;
        public String PasswordHash  { get; set; } = null!;
        public String PasswordSalt  { get; set; } = null!;
        public String Email         { get; set; } = null!;
        public String RealName      { get; set; } = null!;
        public String? Avatar       { get; set; }

        public DateTime RegisterDt { get; set;}
        public DateTime? LastEnterDt { get; set; }
        public String? EmailCode { get; set; }

        /// Добавлено 2023-04-19 робота с Profile
        public Boolean IsEmailPublic { get; set; }
        public Boolean IsRealNamePublic { get; set; }
        public Boolean IsDtPublic { get; set; }
    }
}
