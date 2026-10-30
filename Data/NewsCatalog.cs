using Koi.Web.Models;

namespace Koi.Web.Data;

public static class NewsCatalog
{
    public static IReadOnlyList<NewsArticle> Articles { get; } =
    [
        new(2782, "“CLOCK IN the world with KOI” Global Magnet Badge Collection Challenge", "NEWS", new DateOnly(2025, 12, 31), "/images/pages/upload_image/news/news_1_7917_2746.jpg"),
        new(7931, "With Care", "MEDIA", new DateOnly(2026, 6, 26), "/images/pages/upload_image/news/news_1_6050_1872.jpg"),
        new(7299, "From Nature", "MEDIA", new DateOnly(2026, 4, 23), "/images/pages/upload_image/news/news_1_5807_1584.jpg"),
        new(5289, "FIFTYLAN Opens in Sydney", "NEWS", new DateOnly(2025, 12, 24), "/images/pages/upload_image/news/news_1_5153_7145.jpg"),
        new(5253, "KOI Brunei, It All Starts with This Cup", "NEWS", new DateOnly(2025, 12, 20), "/images/pages/upload_image/news/news_1_1752_6518.jpg"),
        new(2092, "KOI20 Gold Dot Award Design Competition Results", "NEWS", new DateOnly(2025, 3, 27), "/images/pages/upload_image/news/news_695.png"),
        new(1155, "KOI20 Gold Dot Award Design Competition", "NEWS", new DateOnly(2024, 12, 16), "/images/pages/upload_image/news/news_634.png"),
        new(7, "Welcome to the brand-new official website! Feel free to explore and experience the unique charm of KOI!", "NEWS", new DateOnly(2024, 12, 1), "/images/pages/upload_image/news/news_16.jpg"),
        new(514, "2024 Company Video-Your Daily Beat", "MEDIA", new DateOnly(2024, 10, 29), "/images/pages/upload_image/news/news_594.jpg"),
        new(513, "Trademark Transformation Part3：New Beat of Vibrancy.", "MEDIA", new DateOnly(2024, 10, 29), "/images/pages/upload_image/news/news_595.jpg"),
        new(512, "Trademark Transformation Part2：What does our new logo represent?", "MEDIA", new DateOnly(2024, 10, 29), "/images/pages/upload_image/news/news_596.jpg"),
        new(8, "Trademark Transformation Part1：Where we begin?", "MEDIA", new DateOnly(2024, 10, 29), "/images/pages/upload_image/news/news_597.jpg"),
        new(508, "A new Beat of vibrancy", "NEWS", new DateOnly(2024, 10, 28), "/images/pages/upload_image/news/news_593.jpg")
    ];
}
