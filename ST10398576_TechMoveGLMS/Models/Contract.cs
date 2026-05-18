namespace ST10398576_TechMoveGLMS.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        public int ClientId { get; set; }   // required FK
        public Client? Client { get; set; } // navigation, optional

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ContractStatus { get; set; }
        public string ContractServiceLevel { get; set; }
        public string? PdfFilePath { get; set; }

        public ICollection<ServiceRequest>? ServiceRequests { get; set; } // make nullable
    }

}
