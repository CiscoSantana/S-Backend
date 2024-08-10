using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saipher.Domain.Entities
{
    [Table("Users")]
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string? Passwd { get; set; }
        public string Email { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsEnabled { get; set; }
    }
}
