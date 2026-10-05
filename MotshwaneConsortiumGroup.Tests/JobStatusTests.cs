using MotshwaneConsortiumGroup.Models;
using Xunit;

namespace MotshwaneConsortiumGroup.Tests;

/// <summary>Pure logic tests for the Assigned -> In Progress -> Delivered -> Completed rule — no database needed.</summary>
public class JobStatusTests
{
    [Theory]
    [InlineData(JobStatus.Assigned, JobStatus.InProgress)]
    [InlineData(JobStatus.InProgress, JobStatus.Delivered)]
    [InlineData(JobStatus.Delivered, JobStatus.Completed)]
    public void CanTransition_OneStepForward_ReturnsTrue(string from, string to)
    {
        Assert.True(JobStatus.CanTransition(from, to));
    }

    [Theory]
    [InlineData(JobStatus.Assigned, JobStatus.Completed)]
    [InlineData(JobStatus.Assigned, JobStatus.Delivered)]
    [InlineData(JobStatus.InProgress, JobStatus.Assigned)]
    [InlineData(JobStatus.Completed, JobStatus.Assigned)]
    [InlineData(JobStatus.Assigned, JobStatus.Assigned)]
    public void CanTransition_SkippingOrBackward_ReturnsFalse(string from, string to)
    {
        Assert.False(JobStatus.CanTransition(from, to));
    }

    [Fact]
    public void CanTransition_WithAnUnknownStatus_ReturnsFalse()
    {
        Assert.False(JobStatus.CanTransition("NotARealStatus", JobStatus.InProgress));
        Assert.False(JobStatus.CanTransition(JobStatus.Assigned, "NotARealStatus"));
    }
}
