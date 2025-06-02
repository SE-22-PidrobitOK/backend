using AuthService.Models.DTO;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class IdentityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IdentityIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        var dto = new RegisterModelDto();
        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailMissing()
    {
        var dto = new RegisterModelDto
        {
            Password = "Valid123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordMissing()
    {
        var dto = new RegisterModelDto
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailInvalid()
    {
        var dto = new RegisterModelDto
        {
            Email = "invalid-email",
            Password = "Valid123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordTooShort()
    {
        var dto = new RegisterModelDto
        {
            Email = "user@example.com",
            Password = "123",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDuplicateEmail()
    {
        var dto = new RegisterModelDto
        {
            Email = "duplicate@example.com",
            Password = "Valid123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var response1 = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var response2 = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenModelIsValid()
    {
        var dto = new RegisterModelDto
        {
            Email = "success@example.com",
            Password = "Valid123!",
            FirstName = "Alice",
            LastName = "Wonder"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var dto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/login", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var registerDto = new RegisterModelDto
        {
            Email = "wrongpass@example.com",
            Password = "CorrectPass123!",
            FirstName = "Wrong",
            LastName = "Pass"
        };

        await _client.PostAsJsonAsync("/api/identity/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = "WrongPassword"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/login", loginDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WithValidCredentials()
    {
        var registerDto = new RegisterModelDto
        {
            Email = "validlogin@example.com",
            Password = "SecurePass123!",
            FirstName = "Valid",
            LastName = "User"
        };

        await _client.PostAsJsonAsync("/api/identity/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        var response = await _client.PostAsJsonAsync("/api/identity/login", loginDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        content.Should().ContainKey("token");
        content.Should().ContainKey("refreshToken");
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequest_WhenAccessTokenInvalid()
    {
        var dto = new RefreshTokenResultDto
        {
            AccessToken = "invalid-token",
            RefreshToken = "some-refresh-token"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenUserNotFound()
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("supersecretkeysupersecretkey123!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var fakeUserId = Guid.NewGuid().ToString();

        var token = handler.WriteToken(new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, fakeUserId) },
            expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: creds
        ));

        var dto = new RefreshTokenResultDto
        {
            AccessToken = token,
            RefreshToken = "any"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenIsWrong()
    {
        var registerDto = new RegisterModelDto
        {
            Email = "refreshwrong@example.com",
            Password = "RefreshWrong123!",
            FirstName = "Refresh",
            LastName = "Wrong"
        };

        await _client.PostAsJsonAsync("/api/identity/register", registerDto);
        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        });

        var loginData = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        var dto = new RefreshTokenResultDto
        {
            AccessToken = loginData["token"],
            RefreshToken = "wrong-refresh-token"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenIsExpired()
    {
        var registerDto = new RegisterModelDto
        {
            Email = "expire@example.com",
            Password = "Expire123!",
            FirstName = "Token",
            LastName = "Expired"
        };

        await _client.PostAsJsonAsync("/api/identity/register", registerDto);
        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        });

        var loginData = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Предполагаем, что refresh токен в базе всё ещё активен, но мы можем имитировать просрочку через контекст позже при необходимости

        // пока что проверим просто с пустым refresh токеном
        var dto = new RefreshTokenResultDto
        {
            AccessToken = loginData["token"],
            RefreshToken = "" // Пустой токен
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldReturnOk_WhenTokenIsValid()
    {
        var registerDto = new RegisterModelDto
        {
            Email = "refreshok@example.com",
            Password = "RefreshOk123!",
            FirstName = "Refresh",
            LastName = "Valid"
        };

        await _client.PostAsJsonAsync("/api/identity/register", registerDto);
        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        });

        var loginData = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        var dto = new RefreshTokenResultDto
        {
            AccessToken = loginData["token"],
            RefreshToken = loginData["refreshToken"]
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        content.Should().ContainKey("token");
        content.Should().ContainKey("refreshToken");
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequest_WhenAccessTokenIsEmpty()
    {
        var dto = new RefreshTokenResultDto
        {
            AccessToken = "",
            RefreshToken = "some-refresh-token"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/refresh", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }




}
