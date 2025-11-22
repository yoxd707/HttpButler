using HttpButler.Services;

namespace HttpButler.Test;

[TestClass]
public sealed class PathResolveServiceTest
{
    private static readonly dynamic[] ParametersTestData = new dynamic[]
    {
        new {
            userId = "A01",
            photoId = "7d36b9155",
        },
        new {
            productId = "12345",
        },
        new {
            orderId = "98765",
            itemId = 4321,
        },
        new {
            lang = "es-es",
            product = "dotnet",
            category = "csharp",
            group = "language-reference",
            post = "language-versioning",
        },
    };

    [TestMethod]
    [DataRow("/users/A01/photos/7d36b9155", "/users/{userId}/photos/{photoId}", 0)]
    [DataRow("/products/12345/details", "/products/{productId}/details", 1)]
    [DataRow("/orders/98765/items/4321/", "/orders/{orderId}/items/{itemId}/", 2)]
    [DataRow("learn.microsoft.com/es-es/dotnet/csharp/language-reference/language-versioning/", "learn.microsoft.com/{lang}/{product}/{category}/{group}/{post}/", 3)]
    public void ResolveUri_RouteParams_ShouldBeCorrect(string expected, string route, int parametersIndex)
    {
        var pathResolveService = new PathResolveService();
        var parameters = ParametersTestData[parametersIndex];

        var uri = pathResolveService.ResolveUri(route, parameters);
        var result = uri.ToString();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("/users/?userId=A01&photoId=7d36b9155", "/users/", 0)]
    [DataRow("/products?productId=12345", "/products", 1)]
    [DataRow("/orders/?orderId=98765&itemId=4321", "/orders/?{orderId}", 2)]
    [DataRow("learn.microsoft.com/?lang=es-es&product=dotnet&category=csharp&group=language-reference&post=language-versioning", "learn.microsoft.com/?{lang}&{product}&", 3)]
    public void ResolveUri_QueryParams_ShouldBeCorrect(string expected, string route, int parametersIndex)
    {
        var pathResolveService = new PathResolveService();
        var parameters = ParametersTestData[parametersIndex];

        var uri = pathResolveService.ResolveUri(route, parameters);
        var result = uri.ToString();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("/users/A01/?photoId=7d36b9155", "/users/{userId}/", 0)]
    [DataRow("/orders/98765?itemId=4321", "/orders/{orderId}", 2)]
    [DataRow("learn.microsoft.com/es-es/?product=dotnet&category=csharp&group=language-reference&post=language-versioning", "learn.microsoft.com/{lang}/?{product}&", 3)]
    public void ResolveUri_RouteParamsAndQueryParams_ShouldBeCorrect(string expected, string route, int parametersIndex)
    {
        var pathResolveService = new PathResolveService();
        var parameters = ParametersTestData[parametersIndex];

        var uri = pathResolveService.ResolveUri(route, parameters);
        var result = uri.ToString();

        Assert.AreEqual(expected, result);
    }
}
