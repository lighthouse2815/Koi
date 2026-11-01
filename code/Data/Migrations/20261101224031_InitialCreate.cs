using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Koi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ShowOnHome = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeaturedDrinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BackgroundImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedDrinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    File = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Category = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BannerImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HomeImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VideoId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    BodyHtml = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    ShowOnHome = table.Column<bool>(type: "boolean", nullable: false),
                    HomeOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreCities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreCities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drinks_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Hours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NavigationUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AcceptsOrders = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_StoreCities_CityId",
                        column: x => x.CityId,
                        principalTable: "StoreCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoreName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StoreAddress = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Fulfillment = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrinkName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Customization = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,0)", precision: 12, scale: 0, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderItem_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "NewsArticles",
                columns: new[] { "Id", "BannerImage", "BodyHtml", "Category", "Date", "HomeImage", "HomeOrder", "Image", "IsFeatured", "IsPublished", "Revision", "ShowOnHome", "Title", "VideoId" },
                values: new object[,]
                {
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000007"), "/images/pages/upload_image/news/news_18.jpg", "\n     <p>Welcome to the brand-new official website of KOI, where innovation and fun come together, just like our Golden Bubbles—delicious, plump, and full of vitality!<br>\n       Please do take your time to savor this delightful experience and explore the unique charm that belongs to KOI!</p>\n    ", "NEWS", new DateOnly(2024, 12, 1), "/images/news-website.jpg", 1, "/images/pages/upload_image/news/news_16.jpg", false, true, 1, true, "Welcome to the brand-new official website! Feel free to explore and experience the unique charm of KOI!", null },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000008"), "", "\n    ", "MEDIA", new DateOnly(2024, 10, 29), "", 1, "/images/pages/upload_image/news/news_597.jpg", false, true, 1, false, "Trademark Transformation Part1：Where we begin?", "dHWNpRFlHi0" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e000001fc"), "", "\n     <p><img alt=\"\" src=\"/images/pages/upload_image/news/news_634.png\" loading=\"lazy\"></p>\n    ", "NEWS", new DateOnly(2024, 10, 28), "", 1, "/images/pages/upload_image/news/news_593.jpg", false, true, 1, false, "A new Beat of vibrancy", null },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000200"), "", "\n    ", "MEDIA", new DateOnly(2024, 10, 29), "", 1, "/images/pages/upload_image/news/news_596.jpg", false, true, 1, false, "Trademark Transformation Part2：What does our new logo represent?", "qG9bUqpir2w" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000201"), "", "\n    ", "MEDIA", new DateOnly(2024, 10, 29), "", 1, "/images/pages/upload_image/news/news_595.jpg", false, true, 1, false, "Trademark Transformation Part3：New Beat of Vibrancy.", "JuF5_oiIIEs" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000202"), "", "\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"color:null\"><span style=\"font-size:12pt\">Brewing tea with sincerity and dedication, we light up the sparks of surprise in life through every cup of KOI. </span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"color:null\"><span style=\"font-size:12pt\">Built on carefully selected ingredients and perseverance to perfection, we create a delicate blend of aroma and taste with precision in each sip.</span></span></p>\n     <p><span style=\"color:null\"><span style=\"font-size:12.0pt\">With our warm and passionate service, we make your every visit at KOI a delightful and unforgettable&nbsp;one.</span></span><br>\n       &nbsp;</p>\n    ", "MEDIA", new DateOnly(2024, 10, 29), "", 1, "/images/pages/upload_image/news/news_594.jpg", false, true, 1, false, "2024 Company Video-Your Daily Beat", "BmwHrFKmYxY" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000483"), "", "\n     <p style=\"margin-left:0cm; margin-right:0cm\"><img alt=\"\" src=\"/images/pages/upload_image/news/news_634.png\" loading=\"lazy\"></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\">&nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\">A shining journey of golden years, filled with achievements and “Thélicious”memories. Every moment of laughter and joy is like our Golden Bubbles, solid and full. Thank YOU for creating brilliance with us.</span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><br><span style=\"font-size:12pt\"><span style=\"color:black\"><strong>Competition Overview:</strong></span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Design a 2026 KOI20th anniversary logo centered around \"A Bubbling Golden Era\".</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Deadline: February 28, 2025</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Eligibility: Open to KOI fans worldwide. Participants may submit multiple entries, individually or in teams.</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Design Requirements: Design style is open; hand-drawing, digital art, and any format <strong>except AI-generated images </strong>are welcome.</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Submission Format: JPG format, A4 size (21cm x 29.7cm) or larger, with a minimum resolution of 300dpi. Submit entries to events@koicafe.com.</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Email Title Format: KOI20_LOGO_Your Name_Country</span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><br><span style=\"font-size:12pt\"><span style=\"color:black\"><strong>Selection Process:</strong></span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Selection Date: March 2025</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Evaluation Criteria: Theme relevance 40%, originality 30%, and expressiveness 30%</span></span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\"><strong>Prizes:</strong></span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Announcement of Winners: March 2025</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Gold Dot Award: One winner will receive 200 KOI drink vouchers and a trophy.</span></span></p>\n     <p><span style=\"font-size:12pt\"><span style=\"color:black\">-Honorable Award: Up to five winners will receive 20 KOI drink vouchers, exclusive gifts, and certificates.</span></span></p>\n     <p><br><span style=\"font-size:12pt\"><span style=\"color:black\"><strong>Contest Declaration：</strong></span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\">1. The entries submitted must be the original works of the entrants and do not infringe any patents, copyrights, trademarks, and other intellectual property rights of anyone else. The entries have not been published in newspapers, magazines, websites, and other media, has not applied for patents or registered copyrights, has not participated in other competitions, and has not entered commercial channels in any form. The organizer shall not bear the legal responsibility arising from the infringement of the rights of others (or units) by the work, and the legal responsibility shall be borne by the participant.</span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\">2. The copyright of the winning entries belongs to the organizer, and it has exclusive ownership including but not limited to the following ways to exercise copyright: enjoy the right to redesign, produce, sell, display, and publicize the work plan, and the winning participants must fill in the \"Work Copyright Ownership Agreement”.</span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\">3. If the entry does not meet the judging criteria, the prize will be vacant.</span></span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:12pt\"><span style=\"color:black\">4. The organizer reserves the right of final interpretation of this event.</span></span></p>\n    ", "NEWS", new DateOnly(2024, 12, 16), "", 1, "/images/pages/upload_image/news/news_634.png", false, true, 1, false, "KOI20 Gold Dot Award Design Competition", null },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e0000082c"), "", "\n     <p><img alt=\"\" src=\"/images/pages/source/Winner%20Post.jpg\" loading=\"lazy\"></p>\n     <p>Thank you to all participants for your enthusiastic participation! We received over 100 vibrant and creative submissions, each one as precious as Ｇold to us.<br>\n       After careful evaluation, we have selected one Grand Prize Winner and five Honorable&nbsp;Entries.</p>\n     <p>We appreciate everyone who has been part of this Bubbling&nbsp;Golden Era&nbsp;with us. Let’s continue moving forward together and create even more wonderful moments!</p>\n    ", "NEWS", new DateOnly(2025, 3, 27), "", 1, "/images/pages/upload_image/news/news_695.png", false, true, 1, false, "KOI20 Gold Dot Award Design Competition Results", null },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00000ade"), "/images/pages/upload_image/news/news_1_4026_4141.jpg", "\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">\"Clock in\"&nbsp;isn't just the start of a daily routine — it's also a way to mark a memorable journey.<br>\n        From clocking in at work, to collecting footprints around the world, these two meanings quietly meet at KOI.</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">Today, KOI Group has expanded to 17&nbsp;countries and over 700 stores worldwide.<br>\n        Wherever you travel, we hope you’ll come across KOI on your journey, enjoy the familiar taste of home, and feel the warmth of an old friend by your side.</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">To celebrate KOI Group’s 20th Anniversary in 2026, we are launching the Global Limited Magnet Badge Collection Challenge!<br>\n        Each participating country features a unique cultural magnet badge design. Travel with your magnet badges, take photos, clock-in, and collect them to complete your KOI world travel map — and stand a chance to win USD 2,000!</span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\"><strong>Campaign Period: </strong>2026/1/1 – 2026/12/31</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\"><strong>Participating Countries:</strong><br>\n        All KOI stores located in Singapore, Indonesia, Cambodia, Vietnam, Thailand, Malaysia, the Philippines, Bangladesh, Laos, Brunei, China, Japan, the United Arab Emirates, and France, together with KOI Group sub-brand FIFTYLAN stores in the United States,&nbsp;Canada and Australia.</span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">⭐ <strong>Level 1 — Tea Specialist</strong><br>\n        Collect 3 different country magnet badges to unlock the Tea Specialist level.<br>\n        ⚠️For every 3 magnet badges collected, you may redeem 1 drink at any store under KOI Group worldwide with the drink card included in the packaging (no size or topping restrictions). Drink cards must be surrendered to the store after redemption.</span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">⭐⭐ <strong>Level 2 — Global Tea Artisan</strong><br>\n        Collect 8 different country magnet badges and be among the first 20 participants to receive 50 KOI Group drink vouchers.<br>\n        ⚠️Redemption requirement:<br>\n        For each country collected, you must provide a photo of yourself holding the country’s magnet badge in front of a KOI Group store in that respective country. Both the store and magnet badge must be clearly identifiable.</span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\">⭐⭐⭐ <strong>Level 3 — Golden Flavor Grandmaster</strong><br>\n        Collect 12 or more different country magnet badges and be among the first 5 participants to win USD 2,000 in cash.<br>\n        ⚠️Redemption requirement:<br>\n        For each country collected, you must provide a photo of yourself holding the country’s magnet badge in front of a KOI Group store in that respective country. Both the store and magnet badge must be clearly identifiable.</span><br>\n       &nbsp;</p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:11pt\"><strong>Terms &amp; Conditions:</strong><br><span style=\"font-size:14px\">This campaign is organized by KOI </span></span><span style=\"font-size:14px\">CAFÉ GROUP (S) PTE. LTD. (the “Organizer”). Participation in this campaign constitutes acceptance of the following terms and conditions:</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:14px\">1. Campaign Period:<br>\n        January 1, 2026 – December 31, 2026. Late submissions will not be accepted.<br>\n        2. Submission Requirements (Global Tea Artisan&nbsp;&amp; Golden Flavor Grandmaster):<br>\n        - All proof photos must be submitted by December 31, 2026 to: events@koicafe.com<br>\n        - If a photo is unclear, incomplete, or disputed, the Organizer reserves the right to request additional information or reject eligibility.<br>\n        3. Travel Companions:<br>\n        Participants traveling together must individually purchase magnet badges and submit their own photo proof in order to qualify.<br>\n        4. KOI Group Drink Voucher Reward (Global Tea Artisan):<br>\n        The Organizer will issue drink vouchers based on the participant’s country of residence, or provide an equivalent reward where applicable.<br>\n        5. Cash Reward (Golden Flavor Grandmaster):<br>\n        - USD 2,000 (Tax withholding shall be handled in accordance with the laws and regulations of each country. Withholding tax and other statutory deductions will be directly deducted from the bonus amount.)<br>\n        - The Organizer reserves the right to adjust reward distribution in accordance with currency, taxation, or regulatory requirements.<br>\n        6. Fair Participation:<br>\n        - Any falsified, duplicated, borrowed, or otherwise improper submissions will result in disqualification.<br>\n        - Participants are solely responsible for ensuring their submitted photos and information do not infringe on any third-party rights.<br>\n        7. Organizer Rights:<br>\n        - The Organizer reserves the right to amend, suspend, or terminate this campaign, and final announcements shall prevail.<br>\n        - By participating, you consent to providing submitted photos and information for marketing and promotional use by the Organizer.</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm\"><span style=\"font-size:14px\">8.&nbsp;Magnet Badge Availability and Supply Notice：Availability of country-specific magnet badges is limited and subject to individual store inventory. The Organizer does not guarantee continuous availability at all stores throughout the promotion period and shall not provide reservations, restocking guarantees, or any form of compensation in the event that purchase is temporarily unavailable due to inventory depletion, logistics delays, local regulations, force majeure, or other operational factors.</span></p>\n    ", "NEWS", new DateOnly(2025, 12, 31), "/images/news-badges.jpg", 0, "/images/pages/upload_image/news/news_1_7917_2746.jpg", true, true, 1, true, "“CLOCK IN the world with KOI” Global Magnet Badge Collection Challenge", null },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00001485"), "/images/pages/upload_image/news/news_1_1752_6518.jpg", "\n     <p>In a city where sunshine, greenery, and sea breezes meet,<br>\n       KOI Brunei officially opens its doors.<br>\n       With a familiar cup of tea, we invite you to embrace Brunei’s relaxed rhythm.<br>\n       Whether for a quiet break or a moment together,<br>\n       it all begins with this cup — your KOI moment.<br><br><strong>Amber Hills Store​​​</strong><br>\n       12:00-22:00<br>\n       Unit No. 1 , Ground Floor, Block A<br>\n       AMBER HILL, Simpang 127-12, Jalan Lebuh Raya Tungku<br>\n       erected on Lot No. 03143, Kampung Pengkalan Gadong<br>\n       Mukim Gadong B, Bandar Seri Begawan BE3719<br>\n       Negara Brunei Darussalam</p>\n    ", "NEWS", new DateOnly(2025, 12, 20), "", 1, "/images/pages/upload_image/news/news_1_1752_6518.jpg", false, true, 1, false, "KOI Brunei, It All Starts with This Cup", "__qPJrAPQd0" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e000014a9"), "", "\n     <p style=\"margin-left:0cm; margin-right:0cm; text-align:justify\"><span style=\"font-size:16px\">We are delighted to announce the opening of FIFTYLAN Australia at Westfield Sydney, located in the heart of one of the world’s most vibrant cities.</span></p>\n     <p style=\"margin-left:0cm; margin-right:0cm; text-align:justify\"><span style=\"font-size:10.5pt\"><span style=\"font-size:16px\">Surrounded by Sydney’s iconic cityscape—where modern architecture meets coastal energy—our new store offers a welcoming space to pause, refresh, and enjoy a thoughtfully crafted cup of tea. Whether you are exploring the city, heading to work, or taking a moment for yourself, FIFTYLAN is here to be part of your daily rhythm.</span></span><br><br><strong><span style=\"font-size:12.0pt\"><span style=\"color:black\">Westfield Sydney</span></span></strong><br><span style=\"font-size:12.0pt\"><span style=\"color:black\">Ground Floor, Shop 44G, Westfield, 450 George St, Sydney NSW 2000, Australia</span></span><span style=\"font-size:10.5pt\"> </span></p>\n    ", "NEWS", new DateOnly(2025, 12, 24), "", 1, "/images/pages/upload_image/news/news_1_5153_7145.jpg", false, true, 1, false, "FIFTYLAN Opens in Sydney", "WkqHT8ULoYQ" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00001c83"), "", "\n     <p>At KOI, every choice begins at the source.<br>\n       In the mountains, ingredients are raised with time and nurtured by nature. From the earth, they are carefully refined and held to the highest standards, transformed into something that brings a quiet moment of delight.<br>\n       When these journeys come together, balance is created — not by chance, but by choice.</p>\n     <p>&nbsp;</p>\n     <p>Since 2006, this commitment has never changed.<br>\n       This is KOI.</p>\n    ", "MEDIA", new DateOnly(2026, 4, 23), "", 1, "/images/pages/upload_image/news/news_1_5807_1584.jpg", false, true, 1, false, "From Nature", "t3Djx70BMIA" },
                    { new Guid("b6c32d11-6903-af1b-83ff-3c0e00001efb"), "", "\n     <p>Every cup of tea carries a promise of consistency.&nbsp;<br>\n       From rigorous ingredient testing to the precise control of every brewing step, every detail is carefully validated and standardized.<br>\n       Where precision meets dedication, a familiar taste transcends time and distance, remaining the same wherever you are in the world.</p>\n     <p><br>\n       Since 2006, this commitment has never changed.<br>\n       This is KOI.</p>\n    ", "MEDIA", new DateOnly(2026, 6, 26), "", 1, "/images/pages/upload_image/news/news_1_6050_1872.jpg", false, true, 1, false, "With Care", "ooWUH3gPBqU" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_CategoryId_IsPublished_SortOrder",
                table: "Drinks",
                columns: new[] { "CategoryId", "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_IsPublished_SortOrder",
                table: "Menus",
                columns: new[] { "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_IsFeatured",
                table: "NewsArticles",
                column: "IsFeatured",
                unique: true,
                filter: "\"IsFeatured\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_IsPublished_Date",
                table: "NewsArticles",
                columns: new[] { "IsPublished", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_DrinkId",
                table: "OrderItem",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_CreatedAt",
                table: "Orders",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IsPublished_CityId",
                table: "Stores",
                columns: new[] { "IsPublished", "CityId" });

            InitialCatalogSeed.Insert(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "FeaturedDrinks");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Drinks");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "StoreCities");
        }
    }
}
