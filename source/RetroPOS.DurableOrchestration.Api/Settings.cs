namespace RetroPOS.DurableOrchestration.Api
{
    public interface Settings
    {
        public const int MAX_WORKLOADS = 5;
        public const int MAX_REQUESTS_PER_WORKLOAD = 10;
        public const string DURABLE_BINDING_COMPONENT_NAME = "retropos.durable-registration.binding";
        public const string DAPR_SIDECAR_BASEURL = "http://127.0.0.1:7100";
    }
}
