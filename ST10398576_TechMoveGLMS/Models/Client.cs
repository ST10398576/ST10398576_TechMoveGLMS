namespace ST10398576_TechMoveGLMS.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientContactDetails { get; set; }
        public string ClientRegion { get; set; }

        // Navigation property
        public ICollection<Contract>? Contracts { get; set; }  // make nullable
    }

}
