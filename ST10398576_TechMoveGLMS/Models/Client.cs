using System.ComponentModel.DataAnnotations;

namespace ST10398576_TechMoveGLMS.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public string ClientContactDetails { get; set; }
        [Required]
        public string ClientRegion { get; set; }

        public ICollection<Contract>? Contracts { get; set; }  // make nullable
    }

}
