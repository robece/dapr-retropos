namespace RetroPOS.Audit.Api
{
    public class Settings
    {
        public const string WAREHOUSE_STATE_COMPONENT_NAME = "retropos.warehouse.state";
        public const string PRODUCTREGISTRATION_PUBSUB_COMPONENT_NAME = "retropos.product-registration.pubsub";
        public const string PRODUCTREGISTRATION_PUBSUB_TOPIC_NAME = "product-registration";
        public const string DAPR_SIDECAR_BASEURL = "http://127.0.0.1:5100";
    }
}