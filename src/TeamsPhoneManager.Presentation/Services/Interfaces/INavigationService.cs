using System.ComponentModel;

namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for managing navigation between pages.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    string CurrentPage { get; set; }
    void NavigateTo(string page);
}
