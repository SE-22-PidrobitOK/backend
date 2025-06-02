using AuthService.Models;
using System;
using System.Security.Claims;
using Xunit;
using System.IdentityModel.Tokens.Jwt;

public class JwtTokenServiceTests
{
    public JwtTokenServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("JWT_SECRET", "supersecretkeysupersecretkey123!");
        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "60");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidToken()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var jwtService = new JwtTokenService();

        var token = jwtService.GenerateAccessToken(user);


        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturn64ByteBase64Token()
    {

        var jwtService = new JwtTokenService();

        var token = jwtService.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnValidPrincipal()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "expired@example.com"
        };

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("JWT_SECRET", "supersecretkeysupersecretkey123!");
        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "1");

        var jwtService = new JwtTokenService();
        var token = jwtService.GenerateAccessToken(user);
        var principal = jwtService.GetPrincipalFromExpiredToken(token);

        Assert.NotNull(principal);

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        Assert.Equal(user.Id.ToString(), userId);
        Assert.Equal(user.Email, email);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_IfTokenIsInvalid()
    {
        var jwtService = new JwtTokenService();
        var invalidToken = "this.is.not.a.real.token";

        var principal = jwtService.GetPrincipalFromExpiredToken(invalidToken);

        Assert.Null(principal);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainRequiredClaims()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "testclaims@example.com"
        };

        var jwtService = new JwtTokenService();
        var token = jwtService.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        Assert.Equal(user.Id.ToString(), sub);
        Assert.Equal(user.Email, email);
        Assert.False(string.IsNullOrEmpty(jti));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_IfSecretIsInvalid()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "broken@example.com"
        };

        Environment.SetEnvironmentVariable("JWT_SECRET", "short");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "1");

        var jwtService = new JwtTokenService();

        string token;

        try
        {
            token = jwtService.GenerateAccessToken(user);
        }
        catch (ArgumentOutOfRangeException)
        {
            return; 
        }

        var principal = jwtService.GetPrincipalFromExpiredToken(token);
        Assert.Null(principal);
    }

    [Fact]
    public void GenerateAccessToken_ShouldGenerateDifferentTokensEachTime()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "repeat@example.com"
        };

        var jwtService = new JwtTokenService();

        var token1 = jwtService.GenerateAccessToken(user);
        var token2 = jwtService.GenerateAccessToken(user);

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeUniqueOnEachCall()
    {
        var jwtService = new JwtTokenService();

        var token1 = jwtService.GenerateRefreshToken();
        var token2 = jwtService.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }


    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_IfIssuerIsWrong()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "wrongissuer@example.com"
        };

        Environment.SetEnvironmentVariable("JWT_ISSUER", "correct-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("JWT_SECRET", "supersecretkeysupersecretkey123!");
        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "1");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        var jwtService = new JwtTokenService();
        var token = jwtService.GenerateAccessToken(user);

        Environment.SetEnvironmentVariable("JWT_ISSUER", "wrong-issuer");

        var jwtServiceWithWrongIssuer = new JwtTokenService();
        var principal = jwtServiceWithWrongIssuer.GetPrincipalFromExpiredToken(token);

        Assert.Null(principal);
    }


    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_IfTokenIsTampered()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "tampered@example.com"
        };

        var jwtService = new JwtTokenService();

        var token = jwtService.GenerateAccessToken(user);
        var tamperedToken = token.Replace('a', 'b'); // просто меняем символ, чтобы подпись не совпала

        var principal = jwtService.GetPrincipalFromExpiredToken(tamperedToken);

        Assert.Null(principal);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_IfAudienceDoesNotMatch()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "aud@example.com"
        };

        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "correct-audience");
        Environment.SetEnvironmentVariable("JWT_SECRET", "supersecretkeysupersecretkey123!");
        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "1");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        var jwtService = new JwtTokenService();
        var token = jwtService.GenerateAccessToken(user);

        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "wrong-audience");

        var jwtServiceWithWrongAudience = new JwtTokenService();
        var principal = jwtServiceWithWrongAudience.GetPrincipalFromExpiredToken(token);

        Assert.Null(principal);
    }

    [Fact]
    public void GenerateAccessToken_ShouldThrowException_IfSecretMissing()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "brokenenv@example.com"
        };

        Environment.SetEnvironmentVariable("JWT_SECRET", null); 

        Assert.Throws<ArgumentNullException>(() =>
        {
            var jwtService = new JwtTokenService();
            jwtService.GenerateAccessToken(user);
        });

        Environment.SetEnvironmentVariable("JWT_SECRET", "supersecretkeysupersecretkey123!");

    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForEmptyToken()
    {
        var jwtService = new JwtTokenService();
        var principal = jwtService.GetPrincipalFromExpiredToken("");
        Assert.Null(principal);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForNullToken()
    {
        var jwtService = new JwtTokenService();
        var principal = jwtService.GetPrincipalFromExpiredToken(null!);
        Assert.Null(principal);
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectExpiration()
    {
        var user = new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "lifetime@example.com"
        };

        Environment.SetEnvironmentVariable("JWT_TOKEN_LIFETIME", "120");

        var jwtService = new JwtTokenService();
        var token = jwtService.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expires = jwtToken.ValidTo;
        var now = DateTime.UtcNow;

        Assert.InRange(expires, now.AddMinutes(119), now.AddMinutes(121));
    }


}
