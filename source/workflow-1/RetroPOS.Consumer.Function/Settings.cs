namespace RetroPOS.Consumer.Function
{
    public class Settings
    {
        public static string DAPR_HTTP_PORT = string.Empty;
        public static string DAPR_GRPC_PORT = string.Empty;

        public const string CONSUMER_QUEUE_NAME = "internal-queue";

        public static string CONSUMER_STATE_COMPONENT_NAME = string.Empty;
    }
}
