namespace UnitTest.ViewModels;

using System.Threading.Tasks;


using FluentAssertions;

using NSubstitute;

using Xunit;
using System.Linq.Expressions;
using System.Linq;
using System;

using InputWpf.ViewModels;
using InputService.Abstraction;

using Microsoft.Extensions.Configuration;

public class MainViewModelTests
{
    [Fact]
    public void ReadConfigurationTest()
    {
        //Arrange

        var robotService = Substitute.For<IRobotControlService>();
        var configuration = Substitute.For<IConfiguration>();

        configuration["Robot"].Returns("Virtual14");

        var mv = new MainWindowViewModel(robotService, configuration);

        //Act/Assert

        mv.SendTo.Should().Be("Virtual14");
    }

    [Theory]
    [InlineData("0", 100U, 123U, 321U, 321U)]
    [InlineData("180", 100U, 123U, 321U, 321U)]
    [InlineData("90", 100U, 123U, 321U, 123U)]
    public void DriveTest(string direction, uint speed, uint duration, uint fwDuration, uint expectedDuration)
    {
        //Arrange

        var robotService  = Substitute.For<IRobotControlService>();
        var configuration = Substitute.For<IConfiguration>();

        configuration["Robot"].Returns("Virtual14");

        var mv = new MainWindowViewModel(robotService, configuration);
        
        mv.Speed        = speed;
        mv.Duration     = duration;
        mv.FwBwDuration = fwDuration;

        //Act

        mv.DriveCommand.Execute(direction);

        //Assert

        robotService.Received(1).Drive("Virtual14", uint.Parse(direction), speed, expectedDuration);
    }

}