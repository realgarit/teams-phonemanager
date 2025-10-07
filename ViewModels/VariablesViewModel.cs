using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using teams_phonemanager.Models;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

namespace teams_phonemanager.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Variables page. Here you can set the variables that will be used throughout the application.";

        [ObservableProperty]
        private bool _teamsConnected;

        [ObservableProperty]
        private bool _graphConnected;

        public VariablesViewModel()
        {
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            TeamsConnected = _sessionManager.TeamsConnected;
            GraphConnected = _sessionManager.GraphConnected;

            _loggingService.Log("Variables page loaded", LogLevel.Info);
        }

        public PhoneManagerVariables Variables
        {
            get => _mainWindowViewModel?.Variables ?? new PhoneManagerVariables();
            set
            {
                if (_mainWindowViewModel != null)
                {
                    _mainWindowViewModel.Variables = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanProceed => TeamsConnected && GraphConnected;

        [RelayCommand]
        private async Task SaveVariablesToFileAsync()
        {
            try
            {
                var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                downloadsPath = Path.Combine(downloadsPath, "Downloads");
                
                var fileName = $"PhoneManagerVariables_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(downloadsPath, fileName);

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(Variables, jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                _loggingService.Log($"Variables saved to: {filePath}", LogLevel.Info);
                
                MessageBox.Show(
                    $"Variables saved successfully to:\n{filePath}",
                    "Save Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error saving variables: {ex.Message}", LogLevel.Error);
                MessageBox.Show(
                    $"Error saving variables:\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task LoadVariablesFromFileAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Load Variables from File",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var json = await File.ReadAllTextAsync(openFileDialog.FileName);
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var loadedVariables = JsonSerializer.Deserialize<PhoneManagerVariables>(json, jsonOptions);
                    
                    if (loadedVariables != null)
                    {
                        Variables = loadedVariables;
                        _loggingService.Log($"Variables loaded from: {openFileDialog.FileName}", LogLevel.Info);
                        
                        MessageBox.Show(
                            $"Variables loaded successfully from:\n{openFileDialog.FileName}",
                            "Load Successful",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to load variables from the selected file.",
                            "Load Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error loading variables: {ex.Message}", LogLevel.Error);
                MessageBox.Show(
                    $"Error loading variables:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 
