using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace teams_phonemanager.Models
{
    public class PhoneManagerVariables : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // General Variables
        private string _customerLegalName = string.Empty;
        public string CustomerLegalName
        {
            get => _customerLegalName;
            set => SetField(ref _customerLegalName, value);
        }

        private string _msFallbackDomain = string.Empty;
        public string MsFallbackDomain
        {
            get => _msFallbackDomain;
            set => SetField(ref _msFallbackDomain, value);
        }

        private string _customer = string.Empty;
        public string Customer
        {
            get => _customer;
            set => SetField(ref _customer, value);
        }

        private string _customerGroupName = string.Empty;
        public string CustomerGroupName
        {
            get => _customerGroupName;
            set => SetField(ref _customerGroupName, value);
        }

        private string _languageId = "de-DE";
        public string LanguageId
        {
            get => _languageId;
            set => SetField(ref _languageId, value);
        }

        private string _timeZoneId = "W. Europe Standard Time";
        public string TimeZoneId
        {
            get => _timeZoneId;
            set => SetField(ref _timeZoneId, value);
        }

        private string _skuId = "48cc152e-09c5-43e7-bd56-1b9bdeefb101";
        public string SkuId
        {
            get => _skuId;
            set => SetField(ref _skuId, value);
        }

        // Auto-generated variables (read-only)
        public string M365Group => $"m365-{Customer}-{CustomerGroupName}";
        public string RacqUPN => $"racq-{Customer}-{CustomerGroupName}@{MsFallbackDomain}";
        public string RacqDisplayName => $"RACQ-{Customer}-{CustomerGroupName}";
        public string CqDisplayName => $"CQ-{Customer}-{CustomerGroupName}";
        public string RaaaUPN => $"raaa-{Customer}-{CustomerGroupName}@{MsFallbackDomain}";
        public string RaaaDisplayName => $"RAAA-{Customer}-{CustomerGroupName}";
        public string AaDisplayName => $"AA-{Customer}-{CustomerGroupName}";
    }
} 