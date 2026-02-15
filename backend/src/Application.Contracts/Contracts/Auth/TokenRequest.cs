using System.ComponentModel.DataAnnotations;

namespace Application.Contracts
{
    public class TokenRequest 
    {
        [Required]
        public string Token {get; set;} = null!;
        [Required]
        public Guid RefreshToken {get; set;}
    }
}
