using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using OPS.Domain.Entities;
using OPS.Domain.Enums;
using OPS.Infrastructure.Data;
using OPS.Infrastructure.Services;
using OPS.Application.Interfaces;
using OPS.Application.DTOs.Incidents;

namespace OPS.Tests;

public class IncidentServiceTests
{
    private OpsDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<OpsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new OpsDbContext(options);
        
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
    public async Task CreateIncidentAsync_GeneratesTrackingId()
    {
        // Arrange
        var context = GetDbContext();
        var mockRealtime = new Mock<IRealtimeNotificationService>();
        var mockSla = new Mock<ISlaPolicyProvider>();
        var mockLogger = new Mock<ILogger<IncidentService>>();

        var service = new IncidentService(context, mockRealtime.Object, mockSla.Object);

        context.Users.Add(new User { Id = 10, FullName = "Reporter", RoleId = 4 });
        context.Teams.Add(new Team { Id = 1, Name = "Test Team" });
        await context.SaveChangesAsync();

        var dto = new CreateIncidentRequest
        {
            Title = "Test Incident",
            Description = "Something is broken",
            Severity = IncidentSeverity.High,
            Priority = IncidentPriority.High,
            TeamId = 1
        };

        // Act
        var result = await service.CreateIncidentAsync(dto, 10);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("OPS-", result.TrackingId);
        Assert.Equal(IncidentStatus.Open, result.Status);
    }
}
