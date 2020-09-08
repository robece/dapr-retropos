# dapr-retropos

Microservices port configuration					

| services  | http | http-dapr | grpc-dapr | metrics-dapr | daprd command |
|---|---|---|---|---|---|
| RetroPOS.Warehouse.Api | 5000 | 5100 | 5200 | 5300 | daprd -app-id warehouse-service -components-path RetroPOS.Dapr.Components -app-port 5000 -dapr-grpc-port 5200 -dapr-http-port 5100 -metrics-port 5300 --log-level debug |
| RetroPOS.Audit.Api | 6000 | 6100 | 6200 | 6300 | daprd -app-id audit-service -components-path RetroPOS.Dapr.Components -app-port 6000 -dapr-grpc-port 6200 -dapr-http-port 6100 -metrics-port 6300 --log-level debug |