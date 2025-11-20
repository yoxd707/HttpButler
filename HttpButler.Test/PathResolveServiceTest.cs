using HttpButler.Services;

namespace HttpButler.Test;

[TestClass]
public sealed class PathResolveServiceTest
{
    [TestMethod]
    public void TestMethod1()
    {
        var expected = "/users/A01/photos/7d36b9155";

        var pathResolveService = new PathResolveService();

        var route = "/users/{userId}/photos/{photoId}";
        var parameters = new
        {
            userId = "A01",
            photoId = "7d36b9155"
        };

        var uri = pathResolveService.ResolveUri(route, parameters);
        var result = uri.ToString();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod2()
    {
        var expected = "/users/A01/photos";

        var pathResolveService = new PathResolveService();

        var route = "/users/{userId}/photos";
        var parameters = new
        {
            userId = "A01"
        };

        var uri = pathResolveService.ResolveUri(route, parameters);
        var result = uri.ToString();

        Assert.AreEqual(expected, result);
    }
}
