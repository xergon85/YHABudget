using System.ComponentModel;
using YHABudget.Core.MVVM;

namespace YHABudget.Tests.MVVM;

public class ViewModelBaseTests
{
    private class TestViewModel : ViewModelBase
    {
        private string _testProperty = string.Empty;
        public string TestProperty
        {
            get => _testProperty;
            set => SetProperty(ref _testProperty, value);
        }
    }
    
    [Fact]
    public void SetProperty_WhenValueChanges_RaisesPropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        string? changedPropertyName = null;
        
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };
        
        // Act
        viewModel.TestProperty = "New Value";
        
        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(nameof(TestViewModel.TestProperty), changedPropertyName);
        Assert.Equal("New Value", viewModel.TestProperty);
    }
    
    [Fact]
    public void SetProperty_WhenValueDoesNotChange_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var viewModel = new TestViewModel();
        viewModel.TestProperty = "Initial Value";
        var propertyChangedRaised = false;
        
        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };
        
        // Act
        viewModel.TestProperty = "Initial Value";
        
        // Assert
        Assert.False(propertyChangedRaised);
    }
    
    [Fact]
    public void ViewModelBase_ImplementsINotifyPropertyChanged()
    {
        // Arrange & Act
        var viewModel = new TestViewModel();
        
        // Assert
        Assert.IsAssignableFrom<INotifyPropertyChanged>(viewModel);
    }
}
