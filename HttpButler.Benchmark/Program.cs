using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HttpButler.Services;

BenchmarkRunner.Run<PathResolveServiceTest>();

[MemoryDiagnoser]
public class PathResolveServiceTest
{
    private static readonly IPathResolveService _pathResolveService = new PathResolveService();

    private static readonly (string, dynamic)[] _testCases = new (string, dynamic)[]
    {
        (
            "/api/users/{userId}/orders/{orderId}",
            new { userId = 123, orderId = 456, filter = "active" }
        ),
        (
            "/api/users/{userId}/orders/{orderId}/items/{itemId}",
            new { userId = 987, orderId = 654, itemId = 321, includeHistory = true, region = "EU", sort = "desc" }
        ),
        (
            "/api/products/{productId}/reviews/{reviewId}/comments/{commentId}",
            new { productId = 2024, reviewId = 88, commentId = 12, page = 3, pageSize = 50, sentiment = "positive", locale = "es-ES" }
        ),
        (
            "/api/inventory/{warehouseId}/sections/{sectionId}/bins/{binId}",
            new { warehouseId = 42, sectionId = 7, binId = 9012, includeEmpty = false, expand = "dimensions,owner", lastSync = "2025-10-02T13:45:00Z" }
        ),
        (
            "/api/projects/{projectId}/tasks/{taskId}/subtasks/{subtaskId}",
            new { projectId = 1001, taskId = 550, subtaskId = 12, status = "in-progress", assignedTo = 7788, priority = "high", tags = "backend,api" }
        ),
        (
            "/api/payments/{paymentId}/refunds/{refundId}/audit/{auditId}",
            new { paymentId = 555, refundId = 777, auditId = 999, includeSensitive = true, currency = "USD", timezone = "UTC-5", mode = "detailed" }
        ),
        (
            "/api/travel/{tripId}/segments/{segmentId}/checkpoints/{checkpointId}",
            new { tripId = 88888, segmentId = 4444, checkpointId = 222, geo = true, accuracy = "high", format = "extended", returnWarnings = true }
        ),
        (
            "/api/users/{userId}/sessions/{sessionId}/devices/{deviceId}",
            new {
                userId = 112233,
                sessionId = "SESS-998877",
                deviceId = "DEV-554433",
                includeMetadata = true,
                permissions = new[] { "read", "write", "admin" },
                filters = new { os = "Android", version = "14.1", activeOnly = false },
                timestamp = "2025-11-20T18:22:33Z"
            }
        ),
        (
            "/api/catalog/{categoryId}/products/{productId}/variants/{variantId}",
            new {
                categoryId = 9001,
                productId = 3002001,
                variantId = "XL-BLUE",
                locale = "en-US",
                warehouses = new[] { 1, 2, 5, 99 },
                options = new { giftWrap = true, insurance = "premium" },
                debug = false,
                minStock = 0
            }
        ),
        (
            "/api/finance/{accountId}/transactions/{transactionId}/entries/{entryId}",
            new {
                accountId = "AC-778899",
                transactionId = "TX-000112233",
                entryId = 4444,
                range = new { from = "2023-01-01", to = "2025-12-31" },
                currencies = new[] { "USD", "EUR", "JPY" },
                includeReversed = true,
                mode = "audit",
                depth = 5
            }
        ),
        (
            "/api/logistics/{shipmentId}/routes/{routeId}/stops/{stopId}",
            new {
                shipmentId = 991122,
                routeId = 77,
                stopId = 5,
                geo = new { lat = -34.6037, lng = -58.3816, accuracy = "medium" },
                anomalies = new[] { "delay", "temperature-spike" },
                includeProofs = true,
                maxItems = 500,
                timezone = "America/Buenos_Aires"
            }
        ),
        (
            "/api/analytics/{workspaceId}/reports/{reportId}/sections/{sectionId}",
            new {
                workspaceId = "WS-12345",
                reportId = "R-9988",
                sectionId = "S-01-A",
                filters = new { userSegments = new[] { "new", "returning" }, minVisits = 10 },
                renderOptions = new { format = "pdf", theme = "dark", dpi = 300 },
                cache = false,
                correlationId = Guid.NewGuid().ToString()
            }
        ),
        (
            "/api/hr/{companyId}/employees/{employeeId}/records/{recordId}",
            new {
                companyId = 9911,
                employeeId = 712,
                recordId = "REC-AX92",
                includeConfidential = false,
                tags = new[] { "performance", "2025", "quarter4" },
                validation = new { strict = true, schemaVersion = "2.1.0" },
                pagination = new { page = 1, size = 250 },
                snapshot = "latest"
            }
        ),
    };


    [Benchmark]
    public void ResolveAllUri()
    {
        foreach (var testCase in _testCases)
            _pathResolveService.ResolveUri(testCase.Item1, testCase.Item2);
    }

    [Benchmark]
    public void ResolveAllUri_X500()
    {
        for (int i = 0; i < 500; i++)
            foreach (var testCase in _testCases)
                _pathResolveService.ResolveUri(testCase.Item1, testCase.Item2);
    }

}
