namespace ConsulAPI.Options
{
    public class ConsulOptions
    {
        public const string Consul = "Consul";

        public string Address { get; set; }
        public string DataCenter { get; set; }
        public int HealthCheckInterval { get; set; }
        public string HealthCheckAction { get; set; }
        public int HealthCheckTimeOut { get; set; }
        public int HealthCheckDeregisterCriticalServiceAfter { get; set; }
    }
}
