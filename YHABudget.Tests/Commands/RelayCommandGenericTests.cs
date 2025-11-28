using YHABudget.Core.Commands;

namespace YHABudget.Tests.Commands;

public class RelayCommandGenericTests
{
    [Fact]
    public void Execute_CallsActionWithParameter()
    {
        // Arrange
        int receivedValue = 0;
        var command = new RelayCommand<int>(value => receivedValue = value);

        // Act
        command.Execute(42);

        // Assert
        Assert.Equal(42, receivedValue);
    }

    [Fact]
    public void CanExecute_WithNoPredicate_ReturnsTrue()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });

        // Act & Assert
        Assert.True(command.CanExecute("test"));
    }

    [Fact]
    public void CanExecute_WithPredicate_ReturnsPredicateResult()
    {
        // Arrange
        var command = new RelayCommand<int>(
            execute: _ => { },
            canExecute: value => value > 0
        );

        // Act & Assert
        Assert.True(command.CanExecute(5));
        Assert.False(command.CanExecute(-5));
        Assert.False(command.CanExecute(0));
    }

    [Fact]
    public void RaiseCanExecuteChanged_RaisesCanExecuteChangedEvent()
    {
        // Arrange
        var command = new RelayCommand<string>(_ => { });
        bool eventRaised = false;
        command.CanExecuteChanged += (sender, args) => eventRaised = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Execute_WithNullParameter_PassesNullToAction()
    {
        // Arrange
        string? receivedValue = "initial";
        var command = new RelayCommand<string?>(value => receivedValue = value);

        // Act
        command.Execute(null);

        // Assert
        Assert.Null(receivedValue);
    }
}
