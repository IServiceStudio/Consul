namespace ConsulAPI.Options
{
    public class LocalServiceOptions
    {
        public const string LocalService = "LocalService";

        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Weight { get; set; }
    }
}
