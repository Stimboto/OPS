using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using OPS.Domain.Entities;
using OPS.Infrastructure.Data;
using OPS.Infrastructure.Services;
using OPS.Application.DTOs.Auth;

namespace OPS.Tests;

public class AuthServiceTests
{
    private OpsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new OpsDbContext(options);
        
        // Add roles
        context.Roles.AddRange(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Manager" },
            new Role { Id = 3, Name = "Responder" },
            new Role { Id = 4, Name = "Reporter" }
        );
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var context = GetDbContext();
        
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["JwtSettings:Secret"]).Returns("SuperSecretKeyThatIsAtLeast32BytesLongForTesting");
        mockConfig.Setup(c => c["JwtSettings:Issuer"]).Returns("OPS");
        mockConfig.Setup(c => c["JwtSettings:Audience"]).Returns("OPS_Clients");
        mockConfig.Setup(c => c["JwtSettings:ExpiryMinutes"]).Returns("60");

        var authService = new AuthService(context, mockConfig.Object);

        // Add user
        context.Users.Add(new User
        {
            Email = "test@gms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            FullName = "Test User",
            RoleId = 4
        });
        await context.SaveChangesAsync();

        var request = new LoginDto
        {
            Email = "test@gms.com",
            Password = "Password123"
        };

        // Act
        var result = await authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal("test@gms.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var context = GetDbContext();
        var mockConfig = new Mock<IConfiguration>();
        var authService = new AuthService(context, mockConfig.Object);

        var request = new LoginDto
        {
            Email = "nonexistent@gms.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(request));
    }
}
