using Microsoft.AspNetCore.Identity;

namespace AuthService.Models
{
    public class PidrobitokUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileDescription { get; set; }
    }
}
