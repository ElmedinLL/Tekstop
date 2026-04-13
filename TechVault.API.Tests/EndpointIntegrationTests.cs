using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TechVault.API.Tests;

/// <summary>HTTP integration tests against the real pipeline, EF contexts, and SQL Server LocalDB (<see cref="ApiWebApplicationFactory.IntegrationConnectionString"/>).</summary>
[Collection("ApiIntegration")]
public sealed class EndpointIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly ApiWebApplicationFactory _factory;

    public EndpointIntegrationTests(ApiWebApplicationFactory factory) => _factory = factory;

    private HttpClient CreateClient() => _factory.CreateClient();

    private static void SetBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<string> RegisterAndGetAccessTokenAsync(HttpClient client)
    {
        var (_, access) = await RegisterAndGetTokensAsync(client);
        return access;
    }

    private static async Task<(string Email, string AccessToken)> RegisterAndGetTokensAsync(HttpClient client)
    {
        var email = $"it.{Guid.NewGuid():N}@example.com";
        var register = new
        {
            email,
            firstName = "Integration",
            lastName = "Test",
            password = "Password1",
            confirmPassword = "Password1",
        };
        using var reg = await client.PostAsJsonAsync("/api/v1/auth/register", register);
        reg.EnsureSuccessStatusCode();
        var auth = await reg.Content.ReadFromJsonAsync<AuthTokenResponse>(JsonOpts);
        Assert.NotNull(auth?.AccessToken);
        return (email, auth.AccessToken);
    }

    private static async Task<string> LoginAccessTokenAsync(HttpClient client, string email, string password)
    {
        using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthTokenResponse>(JsonOpts);
        Assert.NotNull(auth?.AccessToken);
        return auth.AccessToken;
    }

    private static async Task<(int AddressId, CartLine CartLine)> CreateAddressAndAddCartLineAsync(
        HttpClient client,
        int productId = 1,
        int quantity = 1)
    {
        var addr = new
        {
            label = "Home",
            fullName = "Integration Test",
            line1 = "1 Test St",
            line2 = (string?)null,
            city = "Testville",
            region = "TS",
            postalCode = "12345",
            country = "US",
            phone = (string?)null,
            isDefaultShipping = true,
            isDefaultBilling = true,
        };
        using var createAddr = await client.PostAsJsonAsync("/api/v1/addresses", addr);
        Assert.Equal(HttpStatusCode.Created, createAddr.StatusCode);
        var createdAddr = await createAddr.Content.ReadFromJsonAsync<AddressCreatedResponse>(JsonOpts);
        Assert.NotNull(createdAddr);

        using var addCart = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new { productId, quantity });
        Assert.Equal(HttpStatusCode.OK, addCart.StatusCode);
        var cart = await addCart.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
        var line = Assert.Single(cart?.Lines ?? []);
        return (createdAddr.Id, line);
    }

    [Fact]
    public async Task Health_returns_OK()
    {
        using var client = CreateClient();
        var res = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Anonymous_catalog_and_cart_endpoints_return_OK()
    {
        using var client = CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/categories")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/categories/laptops")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/products")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/products/featured?take=4")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/products/1")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/products/category/laptops")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/cart")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/products/1/reviews")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/WeatherForecast")).StatusCode);

        var coupon = new { code = "NONE", orderSubtotal = 100m };
        var couponRes = await client.PostAsJsonAsync("/api/v1/coupons/validate", coupon);
        Assert.Equal(HttpStatusCode.OK, couponRes.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoints_return_401_without_JWT()
    {
        using var client = CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/addresses")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/wishlist")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/products/1/reviews/me")).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/admin/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/admin/products")).StatusCode);
    }

    [Fact]
    public async Task Auth_register_login_me_refresh_logout_flow()
    {
        using var client = CreateClient();
        var email = $"flow.{Guid.NewGuid():N}@example.com";
        var password = "Password1";

        var register = new
        {
            email,
            firstName = "Flow",
            lastName = "Test",
            password,
            confirmPassword = password,
        };
        using (var reg = await client.PostAsJsonAsync("/api/v1/auth/register", register))
        {
            Assert.Equal(HttpStatusCode.OK, reg.StatusCode);
            var auth = await reg.Content.ReadFromJsonAsync<AuthTokenResponse>(JsonOpts);
            Assert.NotNull(auth?.AccessToken);
            Assert.False(string.IsNullOrEmpty(auth.RefreshToken));
        }

        using (var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password }))
        {
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var auth = await login.Content.ReadFromJsonAsync<AuthTokenResponse>(JsonOpts);
            Assert.NotNull(auth?.AccessToken);
            SetBearer(client, auth.AccessToken);

            using var me = await client.GetAsync("/api/v1/auth/me");
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);

            using var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken });
            Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);

            using var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = auth.RefreshToken });
            Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        }
    }

    [Fact]
    public async Task Customer_JWT_can_use_profile_addresses_wishlist_cart_order_stripe_returns_400_without_stripe()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        using (var me = await client.GetAsync("/api/v1/users/profile"))
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        using (var addresses = await client.GetAsync("/api/v1/addresses"))
            Assert.Equal(HttpStatusCode.OK, addresses.StatusCode);

        var addr = new
        {
            label = "Home",
            fullName = "Integration Test",
            line1 = "1 Test St",
            line2 = (string?)null,
            city = "Testville",
            region = "TS",
            postalCode = "12345",
            country = "US",
            phone = (string?)null,
            isDefaultShipping = true,
            isDefaultBilling = true,
        };
        using (var createAddr = await client.PostAsJsonAsync("/api/v1/addresses", addr))
        {
            Assert.Equal(HttpStatusCode.Created, createAddr.StatusCode);
            var created = await createAddr.Content.ReadFromJsonAsync<AddressCreatedResponse>(JsonOpts);
            Assert.NotNull(created);
            var addressId = created.Id;

            using (var addCart = await client.PostAsJsonAsync("/api/v1/cart/items", new { productId = 1, quantity = 1 }))
            {
                Assert.Equal(HttpStatusCode.OK, addCart.StatusCode);
                var cart = await addCart.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
                Assert.NotNull(cart?.Lines);
                var line = Assert.Single(cart.Lines!);

                var order = new
                {
                    addressId,
                    paymentMethod = "card",
                    couponCode = (string?)null,
                    shippingMethod = "standard",
                    cartItems = new[] { new { productId = line.ProductId, quantity = line.Quantity } },
                };
                using var orderRes = await client.PostAsJsonAsync("/api/v1/orders", order);
                Assert.Equal(HttpStatusCode.Created, orderRes.StatusCode);
                var orderDto = await orderRes.Content.ReadFromJsonAsync<OrderCreatedResponse>(JsonOpts);
                Assert.NotNull(orderDto?.Id);

                var pay = new { amount = orderDto.Total, orderId = orderDto.Id };
                using var payRes = await client.PostAsJsonAsync("/api/v1/payments/create-intent", pay);
                Assert.Equal(HttpStatusCode.BadRequest, payRes.StatusCode);
            }
        }

        using (var wl = await client.PostAsync("/api/v1/wishlist/1", null))
            Assert.Equal(HttpStatusCode.OK, wl.StatusCode);

        using (var wlList = await client.GetAsync("/api/v1/wishlist"))
            Assert.Equal(HttpStatusCode.OK, wlList.StatusCode);

        using (var admin = await client.GetAsync("/api/v1/admin/stats"))
            Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
    }

    [Fact]
    public async Task Payment_webhook_without_config_returns_BadRequest()
    {
        using var client = CreateClient();
        using var res = await client.PostAsync("/api/v1/payments/webhook", new StringContent("{}"));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Refresh_with_invalid_token_returns_401()
    {
        using var client = CreateClient();
        // Validator requires length ≥ 32; use a valid-shaped token that is not in the database.
        using var res = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new { refreshToken = $"{Guid.NewGuid():N}{Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_400()
    {
        using var client = CreateClient();
        var email = $"dup.{Guid.NewGuid():N}@example.com";
        var body = new
        {
            email,
            firstName = "Du",
            lastName = "Pe",
            password = "Password1",
            confirmPassword = "Password1",
        };
        using (var first = await client.PostAsJsonAsync("/api/v1/auth/register", body))
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        using var second = await client.PostAsJsonAsync("/api/v1/auth/register", body);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_wrong_password_returns_401()
    {
        using var client = CreateClient();
        var (email, _) = await RegisterAndGetTokensAsync(client);
        using var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "WrongPass1" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Cart_update_quantity_remove_line_merge_second_product_clear()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        using (var add = await client.PostAsJsonAsync("/api/v1/cart/items", new { productId = 1, quantity = 2 }))
        {
            Assert.Equal(HttpStatusCode.OK, add.StatusCode);
            var cart = await add.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
            Assert.NotNull(cart?.Lines);
            var line = Assert.Single(cart.Lines);
            var itemId = line.CartItemId;

            using (var put = await client.PutAsJsonAsync($"/api/v1/cart/items/{itemId}", new { quantity = 1 }))
            {
                Assert.Equal(HttpStatusCode.OK, put.StatusCode);
                var updated = await put.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
                Assert.Equal(1, Assert.Single(updated?.Lines ?? []).Quantity);
            }

            using (var del = await client.DeleteAsync($"/api/v1/cart/items/{itemId}"))
            {
                Assert.Equal(HttpStatusCode.OK, del.StatusCode);
                var after = await del.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
                Assert.Empty(after?.Lines ?? []);
            }
        }

        using (var addAgain = await client.PostAsJsonAsync("/api/v1/cart/items", new { productId = 1, quantity = 1 }))
            Assert.Equal(HttpStatusCode.OK, addAgain.StatusCode);

        using (var merge = await client.PostAsJsonAsync(
                     "/api/v1/cart/merge",
                     new { lines = new[] { new { productId = 2, quantity = 1 } } }))
        {
            Assert.Equal(HttpStatusCode.OK, merge.StatusCode);
            var merged = await merge.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
            Assert.Equal(2, merged?.Lines?.Count ?? 0);
        }

        using (var clear = await client.DeleteAsync("/api/v1/cart"))
        {
            Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
            var empty = await clear.Content.ReadFromJsonAsync<CartSnapshot>(JsonOpts);
            Assert.Empty(empty?.Lines ?? []);
        }
    }

    [Fact]
    public async Task Address_update_and_delete()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        var addr = new
        {
            label = "Work",
            fullName = "Jane Doe",
            line1 = "9 Oak Ave",
            line2 = (string?)null,
            city = "OldCity",
            region = "OC",
            postalCode = "11111",
            country = "US",
            phone = (string?)null,
            isDefaultShipping = true,
            isDefaultBilling = false,
        };
        using var create = await client.PostAsJsonAsync("/api/v1/addresses", addr);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<AddressCreatedResponse>(JsonOpts);
        Assert.NotNull(created);
        var id = created.Id;

        var update = new
        {
            label = "Work",
            fullName = "Jane Doe",
            line1 = "9 Oak Ave",
            line2 = (string?)null,
            city = "NewCity",
            region = "NC",
            postalCode = "22222",
            country = "US",
            phone = (string?)null,
            isDefaultShipping = true,
            isDefaultBilling = false,
        };
        using var put = await client.PutAsJsonAsync($"/api/v1/addresses/{id}", update);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        using (var list = await client.GetAsync("/api/v1/addresses"))
        {
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            var rows = await list.Content.ReadFromJsonAsync<List<AddressListRow>>(JsonOpts);
            var row = Assert.Single(rows ?? []);
            Assert.Equal("NewCity", row.City);
        }

        using var del = await client.DeleteAsync($"/api/v1/addresses/{id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        using (var list2 = await client.GetAsync("/api/v1/addresses"))
        {
            var rows = await list2.Content.ReadFromJsonAsync<List<AddressListRow>>(JsonOpts);
            Assert.DoesNotContain(rows ?? [], r => r.Id == id);
        }
    }

    [Fact]
    public async Task Orders_list_detail_and_cancel()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        var (addressId, line) = await CreateAddressAndAddCartLineAsync(client);
        var orderBody = new
        {
            addressId,
            paymentMethod = "card",
            couponCode = (string?)null,
            shippingMethod = "standard",
            cartItems = new[] { new { productId = line.ProductId, quantity = line.Quantity } },
        };
        using var orderRes = await client.PostAsJsonAsync("/api/v1/orders", orderBody);
        Assert.Equal(HttpStatusCode.Created, orderRes.StatusCode);
        var placed = await orderRes.Content.ReadFromJsonAsync<OrderCreatedResponse>(JsonOpts);
        Assert.NotNull(placed);

        using (var list = await client.GetAsync("/api/v1/orders"))
        {
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            var result = await list.Content.ReadFromJsonAsync<OrderListJson>(JsonOpts);
            Assert.Contains(result?.Items ?? [], o => o.Id == placed.Id);
        }

        using (var one = await client.GetAsync($"/api/v1/orders/{placed.Id}"))
        {
            Assert.Equal(HttpStatusCode.OK, one.StatusCode);
            var detail = await one.Content.ReadFromJsonAsync<OrderDetailJson>(JsonOpts);
            Assert.Equal(placed.Id, detail?.Id);
        }

        using var cancel = await client.PostAsync($"/api/v1/orders/{placed.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
    }

    [Fact]
    public async Task Wishlist_add_list_remove()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        using (var add = await client.PostAsync("/api/v1/wishlist/3", null))
            Assert.Equal(HttpStatusCode.OK, add.StatusCode);

        using (var list = await client.GetAsync("/api/v1/wishlist"))
        {
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
            var items = await list.Content.ReadFromJsonAsync<List<WishlistRow>>(JsonOpts);
            Assert.Contains(items ?? [], w => w.ProductId == 3);
        }

        using (var del = await client.DeleteAsync("/api/v1/wishlist/3"))
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        using (var list2 = await client.GetAsync("/api/v1/wishlist"))
        {
            var items = await list2.Content.ReadFromJsonAsync<List<WishlistRow>>(JsonOpts);
            Assert.DoesNotContain(items ?? [], w => w.ProductId == 3);
        }
    }

    [Fact]
    public async Task Review_create_and_delete_after_purchase()
    {
        using var client = CreateClient();
        var token = await RegisterAndGetAccessTokenAsync(client);
        SetBearer(client, token);

        var (addressId, line) = await CreateAddressAndAddCartLineAsync(client, productId: 1, quantity: 1);
        using var orderRes = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new
            {
                addressId,
                paymentMethod = "card",
                couponCode = (string?)null,
                shippingMethod = "standard",
                cartItems = new[] { new { productId = line.ProductId, quantity = line.Quantity } },
            });
        Assert.Equal(HttpStatusCode.Created, orderRes.StatusCode);

        using (var postReview = await client.PostAsJsonAsync(
                   "/api/v1/products/1/reviews",
                   new { rating = 5, comment = "Integration test review", title = (string?)null }))
            Assert.Equal(HttpStatusCode.Created, postReview.StatusCode);

        using (var me = await client.GetAsync("/api/v1/products/1/reviews/me"))
        {
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            var status = await me.Content.ReadFromJsonAsync<MyReviewStatusJson>(JsonOpts);
            Assert.True(status?.Purchased);
            Assert.NotNull(status?.Review);
        }

        using (var del = await client.DeleteAsync("/api/v1/products/1/reviews"))
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        using (var postAgain = await client.PostAsJsonAsync(
                   "/api/v1/products/1/reviews",
                   new { rating = 4, comment = "Second review", title = (string?)null }))
            Assert.Equal(HttpStatusCode.Created, postAgain.StatusCode);
    }

    [Fact]
    public async Task Admin_login_can_access_stats_and_products()
    {
        using var client = CreateClient();
        var token = await LoginAccessTokenAsync(
            client,
            ApiWebApplicationFactory.IntegrationAdminEmail,
            ApiWebApplicationFactory.IntegrationAdminPassword);
        SetBearer(client, token);

        using (var stats = await client.GetAsync("/api/v1/admin/stats"))
            Assert.Equal(HttpStatusCode.OK, stats.StatusCode);

        using (var products = await client.GetAsync("/api/v1/admin/products"))
            Assert.Equal(HttpStatusCode.OK, products.StatusCode);
    }

    private sealed class AuthTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    private sealed class AddressCreatedResponse
    {
        public int Id { get; set; }
    }

    private sealed class CartSnapshot
    {
        public List<CartLine>? Lines { get; set; }
    }

    private sealed class CartLine
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private sealed class OrderCreatedResponse
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
    }

    private sealed class AddressListRow
    {
        public int Id { get; set; }
        public string City { get; set; } = null!;
    }

    private sealed class OrderListJson
    {
        public List<OrderSummaryJson>? Items { get; set; }
    }

    private sealed class OrderSummaryJson
    {
        public int Id { get; set; }
    }

    private sealed class OrderDetailJson
    {
        public int Id { get; set; }
    }

    private sealed class WishlistRow
    {
        public int ProductId { get; set; }
    }

    private sealed class MyReviewStatusJson
    {
        public bool Purchased { get; set; }
        public object? Review { get; set; }
    }
}
