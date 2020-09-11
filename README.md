# Dapr Retro Point of Sales

<div style="text-align:center">
    <img src="/resources/images/architecture.png" />
</div>

<div style="text-align:center">
    <img src="/resources/images/screen01.png" />
</div>

<div style="text-align:center">
    <img src="/resources/images/screen02.png" />
</div>

<div style="text-align:center">
    <img src="/resources/images/screen03.png" />
</div>

## Microservices port configuration

| services  | http | http-dapr | grpc-dapr | metrics-dapr | daprd command |
|---|---|---|---|---|---|
| RetroPOS.Warehouse.Api | 5000 | 5100 | 5200 | 5300 | daprd -app-id warehouse-service -components-path source\RetroPOS.Dapr.Components -app-port 5000 -dapr-grpc-port 5200 -dapr-http-port 5100 -metrics-port 5300 -log-level debug -config retropos.observability.tracing.yml |
| RetroPOS.Audit.Api | 6000 | 6100 | 6200 | 6300 | daprd -app-id audit-service -components-path source\RetroPOS.Dapr.Components -app-port 6000 -dapr-grpc-port 6200 -dapr-http-port 6100 -metrics-port 6300 -log-level debug -config retropos.observability.tracing.yml |