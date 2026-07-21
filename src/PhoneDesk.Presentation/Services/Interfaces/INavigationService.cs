using System.ComponentModel;

namespace PhoneDesk.Services.Interfaces;

/// <summary>
/// Service for managing navigation between pages.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    string CurrentPage { get; set; }
    void NavigateTo(string page);
}
