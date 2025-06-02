using AuthService.Models;

public static class FakeUserData
{
    public static PidrobitokUser GetTestUser()
    {
        return new PidrobitokUser
        {
            Id = Guid.NewGuid(),
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User",
            RefreshToken = "refresh",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(5)
        };
    }
}
