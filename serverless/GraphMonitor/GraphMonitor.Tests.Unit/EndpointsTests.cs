//using FluentAssertions;
//using Microsoft.AspNetCore.Http.HttpResults;
//using NSubstitute;
//using StackExchange.Redis;

//namespace GraphMonitor.Tests.Unit;

//public class EndpointsTests {
    
//    [Theory]
//    [InlineData("test")]
//    public async Task Get_ReturnsNotFoundForMissingData(string name) {
//        var redis = Substitute.For<IConnectionMultiplexer>();
//        var db = Substitute.For<IDatabase>();
//        redis.GetDatabase()
//            .Returns(db);
//        db.StringGetAsync($"graph:{name}")
//            .Returns(new RedisValue());

//        var result = await Endpoints.GetUrl(name, redis);

//        result.Should()
//            .BeOfType<NotFound<string>>();
//    }

//    [Theory]
//    [InlineData("test")]
//    public async Task Get_ReturnsOkForFoundData(string name) {
//        var redis = Substitute.For<IConnectionMultiplexer>();
//        var db = Substitute.For<IDatabase>();
//        redis.GetDatabase()
//            .Returns(db);
//        db.StringGetAsync($"graph:{name}")
//            .Returns(new RedisValue("test"));

//        var result = await Endpoints.GetUrl(name, redis);

//        result.Should()
//            .BeOfType<ContentHttpResult>();
//        var content = result as ContentHttpResult;
//        content?.ResponseContent.Should()
//            .Be("test");
//    }

//    [Theory]
//    [InlineData("test")]
//    public async Task Store_StoresData(string name) {
//        var redis = Substitute.For<IConnectionMultiplexer>();
//        var db = Substitute.For<IDatabase>();
//        redis.GetDatabase()
//            .Returns(db);
//        using var stream = new MemoryStream("test"u8.ToArray());

//        var result = await Endpoints.StoreUrl(name, stream, redis);

//        result.Should().BeOfType<Ok>();
//        await db.Received(1)
//            .StringSetAsync($"graph:{name}", "test");
//    }
//}



