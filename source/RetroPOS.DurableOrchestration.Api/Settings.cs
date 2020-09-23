namespace RetroPOS.DurableOrchestration.Api
{
    public interface Settings
    {
        public static string DAPR_HTTP_PORT = string.Empty;
        public static string DAPR_GRPC_PORT = string.Empty;

        public static int MAX_WORKLOADS = 0;
        public static int MAX_REQUESTS_PER_WORKLOAD = 0;
        public static string DURABLE_BINDING_COMPONENT_NAME = string.Empty;
    }
}
