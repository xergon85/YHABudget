using YHABudget.Core.Commands;

namespace YHABudget.Tests.Commands;

public class RelayCommandTests
{
    [Fact]
    public void Execute_ExecutesAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);
        
        // Act
        command.Execute(null);
        
        // Assert
        Assert.True(executed);
    }
    
    [Fact]
    public void CanExecute_WhenNoCanExecuteProvided_ReturnsTrue()
    {
        // Arrange
        var command = new RelayCommand(() => { });
        
        // Act
        var canExecute = command.CanExecute(null);
        
        // Assert
        Assert.True(canExecute);
    }
    
    [Fact]
    public void CanExecute_WhenCanExecuteProvided_ReturnsCanExecuteResult()
    {
        // Arrange
        var canExecuteFlag = false;
        var command = new RelayCommand(() => { }, () => canExecuteFlag);
        
        // Act & Assert - Initially false
        Assert.False(command.CanExecute(null));
        
        // Act & Assert - After changing to true
        canExecuteFlag = true;
        Assert.True(command.CanExecute(null));
    }
    
    [Fact]
    public void Execute_WhenCanExecuteIsFalse_DoesNotExecute()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true, () => false);
        
        // Act
        if (command.CanExecute(null))
        {
            command.Execute(null);
        }
        
        // Assert
        Assert.False(executed);
    }
}
