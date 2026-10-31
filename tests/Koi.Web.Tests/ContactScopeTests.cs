using AngleSharp.Html.Parser;
using Xunit;

namespace Koi.Web.Tests;

public class ContactScopeTests(NewsApplicationFactory factory) : IClassFixture<NewsApplicationFactory>
{
    [Fact]
    public async Task ContactPageKeepsLocalContactAndOmitsOverseasPartnership()
    {
        using var client = AdminTestClient.Anonymous(factory);
        var document = await new HtmlParser().ParseDocumentAsync(await client.GetStringAsync("/Home/Contact"));
        Assert.NotNull(document.QuerySelector("#contactForm1"));
        Assert.NotNull(document.QuerySelector("#contactForm1 input[name=name]"));
        Assert.Null(document.QuerySelector("#area"));
        Assert.Null(document.QuerySelector("#contactForm2"));
        Assert.Null(document.QuerySelector("[data-contact-type='2']"));
    }
}
