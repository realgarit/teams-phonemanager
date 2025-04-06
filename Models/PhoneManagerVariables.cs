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

        private string _usageLocation = "CH";
        public string UsageLocation
        {
            get => _usageLocation;
            set => SetField(ref _usageLocation, value);
        }

        private string _skuId = "440eaaa8-b3e0-484b-a8be-62870b9ba70a";
        public string SkuId
        {
            get => _skuId;
            set => SetField(ref _skuId, value);
        }

        // Call Queues Variables
        private string _csAppCqId = "11cd3e2e-fccb-42ad-ad00-878b93575e07";
        public string CsAppCqId
        {
            get => _csAppCqId;
            set => SetField(ref _csAppCqId, value);
        }

        // Auto Attendants Variables
        private string _csAppAaId = "ce933385-9390-45d1-9512-c8d228074e07";
        public string CsAppAaId
        {
            get => _csAppAaId;
            set => SetField(ref _csAppAaId, value);
        }

        private string _raaAnr = string.Empty;
        public string RaaAnr
        {
            get => _raaAnr;
            set => SetField(ref _raaAnr, value);
        }

        private string _phoneNumberType = "DirectRouting";
        public string PhoneNumberType
        {
            get => _phoneNumberType;
            set => SetField(ref _phoneNumberType, value);
        }

        private string _defaultCallFlowGreetingPromptDE = string.Empty;
        public string DefaultCallFlowGreetingPromptDE
        {
            get => _defaultCallFlowGreetingPromptDE;
            set => SetField(ref _defaultCallFlowGreetingPromptDE, value);
        }

        private string _afterHoursCallFlowGreetingPromptDE = string.Empty;
        public string AfterHoursCallFlowGreetingPromptDE
        {
            get => _afterHoursCallFlowGreetingPromptDE;
            set => SetField(ref _afterHoursCallFlowGreetingPromptDE, value);
        }

        private TimeSpan _openingHours1Start = new TimeSpan(8, 0, 0);
        public TimeSpan OpeningHours1Start
        {
            get => _openingHours1Start;
            set => SetField(ref _openingHours1Start, value);
        }

        private TimeSpan _openingHours1End = new TimeSpan(12, 0, 0);
        public TimeSpan OpeningHours1End
        {
            get => _openingHours1End;
            set => SetField(ref _openingHours1End, value);
        }

        private TimeSpan _openingHours2Start = new TimeSpan(13, 30, 0);
        public TimeSpan OpeningHours2Start
        {
            get => _openingHours2Start;
            set => SetField(ref _openingHours2Start, value);
        }

        private TimeSpan _openingHours2End = new TimeSpan(17, 0, 0);
        public TimeSpan OpeningHours2End
        {
            get => _openingHours2End;
            set => SetField(ref _openingHours2End, value);
        }

        // Holidays Variables
        private string _holidayName = string.Empty;
        public string HolidayName
        {
            get => _holidayName;
            set => SetField(ref _holidayName, value);
        }

        private string _holidayGreetingPromptDE = string.Empty;
        public string HolidayGreetingPromptDE
        {
            get => _holidayGreetingPromptDE;
            set => SetField(ref _holidayGreetingPromptDE, value);
        }

        private DateTime _holidayDate = DateTime.Now;
        public DateTime HolidayDate
        {
            get => _holidayDate;
            set => SetField(ref _holidayDate, value);
        }

        // Auto-generated variables (read-only)
        public string M365Group => $"ttgrp-{Customer}-{CustomerGroupName}";
        public string RacqUPN => $"racq-{Customer}-{CustomerGroupName}{MsFallbackDomain}";
        public string RacqDisplayName => $"racq-{Customer}-{CustomerGroupName}";
        public string CqDisplayName => $"cq-{Customer}-{CustomerGroupName}";
        public string RaaaUPN => $"raaa-{Customer}-hn-{CustomerGroupName}{MsFallbackDomain}";
        public string RaaaDisplayName => $"raaa-{Customer}-hn-{CustomerGroupName}";
        public string AaDisplayName => $"aa-{Customer}-hn-{CustomerGroupName}";
    }
} 