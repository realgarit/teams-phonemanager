using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class PhoneManagerVariables : ObservableObject
    {
        [ObservableProperty]
        private string _groupName = string.Empty;

        [ObservableProperty]
        private string _groupDescription = string.Empty;

        [ObservableProperty]
        private string _customer = string.Empty;

        [ObservableProperty]
        private string _customerGroupName = string.Empty;

        [ObservableProperty]
        private string _msFallbackDomain = string.Empty;

        [ObservableProperty]
        private string _raaAnrName = string.Empty;

        [ObservableProperty]
        private string _customerLegalName = string.Empty;

        [ObservableProperty]
        private string _languageId = string.Empty;

        [ObservableProperty]
        private string _timeZoneId = string.Empty;

        [ObservableProperty]
        private string _usageLocation = string.Empty;

        [ObservableProperty]
        private string _skuId = "440eaaa8-b3e0-484b-a8be-62870b9ba70a";

        [ObservableProperty]
        private string _csAppCqId = "11cd3e2e-fccb-42ad-ad00-878b93575e07";

        [ObservableProperty]
        private string _csAppAaId = "ce933385-9390-45d1-9512-c8d228074e07";

        [ObservableProperty]
        private string _raaAnr = string.Empty;

        [ObservableProperty]
        private string _phoneNumberType = string.Empty;

        [ObservableProperty]
        private string _defaultCallFlowGreetingPromptDE = string.Empty;

        [ObservableProperty]
        private string _afterHoursCallFlowGreetingPromptDE = string.Empty;

        [ObservableProperty]
        private TimeSpan _openingHours1Start = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private TimeSpan _openingHours1End = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private TimeSpan _openingHours2Start = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private TimeSpan _openingHours2End = new TimeSpan(0, 0, 0);

        [ObservableProperty]
        private string _holidayNameSuffix = string.Empty;

        [ObservableProperty]
        private string _holidayGreetingPromptDE = string.Empty;

        [ObservableProperty]
        private DateTime _holidayDate = DateTime.Now;

        // Auto-generated variables (read-only)
        public string M365Group => $"ttgrp-{Customer}-{CustomerGroupName}";
        public string RacqUPN => $"racq-{Customer}-{CustomerGroupName}{MsFallbackDomain}";
        public string RacqDisplayName => $"racq-{Customer}-{CustomerGroupName}";
        public string CqDisplayName => $"cq-{Customer}-{CustomerGroupName}";
        public string RaaaUPN => $"raaa-{Customer}-{RaaAnrName}-{CustomerGroupName}{MsFallbackDomain}";
        public string RaaaDisplayName => $"raaa-{Customer}-{RaaAnrName}-{CustomerGroupName}";
        public string AaDisplayName => $"aa-{Customer}-{RaaAnrName}-{CustomerGroupName}";

        public string HolidayName
        {
            get => $"hd-{Customer}-{HolidayNameSuffix}";
            set
            {
                string prefix = $"hd-{Customer}-";
                string newValue = value;

                // Remove any duplicate prefixes that might have been added
                while (newValue.StartsWith("hd-"))
                {
                    int nextDash = newValue.IndexOf('-', 3);
                    if (nextDash == -1) break;
                    
                    string potentialCustomer = newValue.Substring(3, nextDash - 3);
                    if (potentialCustomer == Customer)
                    {
                        newValue = newValue.Substring(nextDash + 1);
                    }
                    else break;
                }

                HolidayNameSuffix = newValue;
                OnPropertyChanged();
            }
        }

        partial void OnCustomerChanged(string value)
        {
            OnPropertyChanged(nameof(M365Group));
            OnPropertyChanged(nameof(RacqUPN));
            OnPropertyChanged(nameof(RacqDisplayName));
            OnPropertyChanged(nameof(CqDisplayName));
            OnPropertyChanged(nameof(RaaaUPN));
            OnPropertyChanged(nameof(RaaaDisplayName));
            OnPropertyChanged(nameof(AaDisplayName));
            OnPropertyChanged(nameof(HolidayName));
        }

        partial void OnCustomerGroupNameChanged(string value)
        {
            OnPropertyChanged(nameof(M365Group));
            OnPropertyChanged(nameof(RacqUPN));
            OnPropertyChanged(nameof(RacqDisplayName));
            OnPropertyChanged(nameof(CqDisplayName));
            OnPropertyChanged(nameof(RaaaUPN));
            OnPropertyChanged(nameof(RaaaDisplayName));
            OnPropertyChanged(nameof(AaDisplayName));
        }

        partial void OnMsFallbackDomainChanged(string value)
        {
            OnPropertyChanged(nameof(RacqUPN));
            OnPropertyChanged(nameof(RaaaUPN));
        }

        partial void OnRaaAnrNameChanged(string value)
        {
            OnPropertyChanged(nameof(RaaaUPN));
            OnPropertyChanged(nameof(RaaaDisplayName));
            OnPropertyChanged(nameof(AaDisplayName));
        }

        partial void OnHolidayNameSuffixChanged(string value)
        {
            OnPropertyChanged(nameof(HolidayName));
        }
    }
} 