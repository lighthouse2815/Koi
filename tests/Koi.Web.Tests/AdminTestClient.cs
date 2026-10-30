using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Koi.Web.Tests;

internal static class AdminTestClient
{
    public static readonly byte[] Image = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jP1cAAAAASUVORK5CYII=");
    public static HttpClient Anonymous(NewsApplicationFactory factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    public static async Task<string> Token(HttpClient client, string url)
    {
        var response = await client.GetAsync(url); response.EnsureSuccessStatusCode();
        var document = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());
        return document.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;
    }
    public static async Task<HttpClient> Login(NewsApplicationFactory factory)
    {
        var client = Anonymous(factory);
        var result = await Post(client, "/Admin/Account/Login", new() { ["Email"] = factory.AdminEmail, ["Password"] = factory.AdminPassword });
        Assert.Equal(HttpStatusCode.Redirect, result.StatusCode); return client;
    }
    public static MultipartFormDataContent Form(Dictionary<string, string> fields, byte[]? image = null, string fileName = "test.png")
    {
        var form = new MultipartFormDataContent();
        foreach (var field in fields) form.Add(new StringContent(field.Value), field.Key);
        if (image != null) form.Add(new ByteArrayContent(image), "ImageUpload", fileName);
        return form;
    }
    public static async Task<HttpResponseMessage> Post(HttpClient client, string url, Dictionary<string, string> fields, byte[]? image = null, string? tokenUrl = null)
    {
        fields["__RequestVerificationToken"] = await Token(client, tokenUrl ?? url);
        return await client.PostAsync(url, Form(fields, image));
    }
}
