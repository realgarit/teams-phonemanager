using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace teams_phonemanager.ViewModels
{
    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _customerLegalName = "Acme Corp";

        [ObservableProperty]
        private string _msFallbackDomain = "@acme.onmicrosoft.com";

        [ObservableProperty]
        private string _customer = "acm-luc";

        [ObservableProperty]
        private string _customerGroupName = "immo";

        [ObservableProperty]
        private string _languageId = "de-DE";

        [ObservableProperty]
        private string _timeZoneId = "W. Europe Standard Time";

        [ObservableProperty]
        private string _usageLocation = "CH";

        [ObservableProperty]
        private string _skuId = "440eaaa8-b3e0-484b-a8be-62870b9ba70a";

        // Call Queues
        [ObservableProperty]
        private string _csAppCqId = "11cd3e2e-fccb-42ad-ad00-878b93575e07";

        // Auto Attendants
        [ObservableProperty]
        private string _csAppAaId = "ce933385-9390-45d1-9512-c8d228074e07";

        [ObservableProperty]
        private string _phoneNumberType = "DirectRouting";

        [ObservableProperty]
        private string _raaAnrName = "hn";

        [ObservableProperty]
        private string _raaAnr = "+41413290024";

        [ObservableProperty]
        private string _defaultCallFlowGreetingPromptDE = "Herzlich Willkommen bei der Acme Corp!";

        [ObservableProperty]
        private string _afterHoursCallFlowGreetingPromptDE = "Herzlich Willkommen bei der Acme Corp, Sie rufen ausserhalb der Öffnungszeiten.";

        [ObservableProperty]
        private TimeSpan? _openingHours1Start = new TimeSpan(8, 0, 0);

        [ObservableProperty]
        private TimeSpan? _openingHours1End = new TimeSpan(12, 0, 0);

        [ObservableProperty]
        private TimeSpan? _openingHours2Start = new TimeSpan(13, 30, 0);

        [ObservableProperty]
        private TimeSpan? _openingHours2End = new TimeSpan(17, 0, 0);

        // Holidays
        [ObservableProperty]
        private string _holidayNameSuffix = "kantonal-2025";

        [ObservableProperty]
        private string _holidayGreetingPromptDE = "Herzlich Willkommen bei der Acme Corp. Leider können wir Ihren Anruf derzeit nicht entgegennehmen.";

        public ObservableCollection<string> HolidaySeries { get; } = new()
        {
            "New Year's Day - 01.01.2025",
            "Berchtoldstag - 02.01.2025",
            "Good Friday - 18.04.2025",
            "Easter Monday - 21.04.2025",
            "Ascension Day - 29.05.2025",
            "Whit Monday - 09.06.2025",
            "National Day - 01.08.2025",
            "Christmas Day - 25.12.2025",
            "St. Stephen's Day - 26.12.2025"
        };

        // Auto-generated (Read-only placeholders)
        [ObservableProperty]
        private string _m365Group = "ttgrp-acm-luc-immo";

        [ObservableProperty]
        private string _racqUPN = "racq-acm-luc-immo@acme.onmicrosoft.com";

        [ObservableProperty]
        private string _racqDisplayName = "racq-acm-luc-immo";

        [ObservableProperty]
        private string _cqDisplayName = "cq-acm-luc-immo";

        [ObservableProperty]
        private string _raaaUPN = "raaa-acm-luc-immo@acme.onmicrosoft.com";

        [ObservableProperty]
        private string _raaaDisplayName = "raaa-acm-luc-immo";

        [ObservableProperty]
        private string _aaDisplayName = "aa-acm-luc-immo";

        public TestViewModel()
        {
        }
    }
}
