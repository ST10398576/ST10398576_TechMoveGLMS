namespace ST10398576_TechMoveGLMS.Models
{
    public class DashboardSummary
    {
        // Total number of clients in the system
        public int TotalClients { get; set; }

        // Total number of contracts
        public int TotalContracts { get; set; }

        // Number of active contracts
        public int ActiveContracts { get; set; }

        // Number of expired contracts
        public int ExpiredContracts { get; set; }

        // Total number of service requests
        public int TotalServiceRequests { get; set; }

        // Number of pending service requests
        public int PendingRequests { get; set; }

        // Number of completed service requests
        public int CompletedRequests { get; set; }
    }
}
