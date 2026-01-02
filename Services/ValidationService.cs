using System.Text.RegularExpressions;
using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public partial class ValidationService : IValidationService
    {
        private readonly ISessionManager _sessionManager;

        [GeneratedRegex(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled)]
        private static partial Regex E164PhonePattern();

        public ValidationService(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public ValidationResult ValidatePrerequisites()
        {
            var result = new ValidationResult();

            if (!_sessionManager.ModulesChecked)
            {
                result.AddError("PowerShell modules have not been checked. Please check modules first.");
            }

            if (!_sessionManager.TeamsConnected)
            {
                result.AddError("Not connected to Microsoft Teams. Please connect to Teams first.");
            }

            if (!_sessionManager.GraphConnected)
            {
                result.AddError("Not connected to Microsoft Graph. Please connect to Graph first.");
            }

            return result;
        }

        public ValidationResult ValidateVariables(PhoneManagerVariables variables)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(variables.Customer))
            {
                result.AddError("Customer name is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.CustomerGroupName))
            {
                result.AddError("Customer group name is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.MsFallbackDomain))
            {
                result.AddError("Microsoft fallback domain is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.CustomerLegalName))
            {
                result.AddError("Customer legal name is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.LanguageId))
            {
                result.AddError("Language ID is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.TimeZoneId))
            {
                result.AddError("Time zone ID is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.UsageLocation))
            {
                result.AddError("Usage location is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.RaaAnr))
            {
                result.AddError("Resource account phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.PhoneNumberType))
            {
                result.AddError("Phone number type is required.");
            }

            // Validate Default Call Flow Greeting
            if (variables.AaDefaultGreetingType == "TextToSpeech" && string.IsNullOrWhiteSpace(variables.AaDefaultGreetingTextToSpeechPrompt))
            {
                result.AddError("Default call flow greeting text is required when using Text-to-Speech.");
            }
            else if (variables.AaDefaultGreetingType == "AudioFile" && string.IsNullOrWhiteSpace(variables.AaDefaultGreetingAudioFileId))
            {
                result.AddError("Default call flow audio file is required when using Audio File.");
            }

            // Validate After Hours Call Flow Greeting
            if (variables.AaAfterHoursGreetingType == "TextToSpeech" && string.IsNullOrWhiteSpace(variables.AaAfterHoursGreetingTextToSpeechPrompt))
            {
                result.AddError("After hours call flow greeting text is required when using Text-to-Speech.");
            }
            else if (variables.AaAfterHoursGreetingType == "AudioFile" && string.IsNullOrWhiteSpace(variables.AaAfterHoursGreetingAudioFileId))
            {
                result.AddError("After hours call flow audio file is required when using Audio File.");
            }

            if (string.IsNullOrWhiteSpace(variables.HolidayNameSuffix))
            {
                result.AddError("Holiday name suffix is required.");
            }

            if (string.IsNullOrWhiteSpace(variables.HolidayGreetingPromptDE))
            {
                result.AddError("Holiday greeting prompt is required.");
            }

            return result;
        }

        public ValidationResult ValidateHolidayDate(DateTime holidayDate)
        {
            var result = new ValidationResult();

            if (holidayDate < DateTime.Now.AddDays(-1))
            {
                result.AddError("Holiday date cannot be in the past.");
            }

            return result;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);

                // Additional validation checks
                if (addr.Address != email)
                    return false;

                // Check for double dots
                if (email.Contains(".."))
                    return false;

                // Check for leading or trailing dots
                if (email.StartsWith(".") || email.EndsWith("."))
                    return false;

                // Check domain has valid TLD
                var parts = email.Split('@');
                if (parts.Length != 2)
                    return false;

                if (!parts[1].Contains('.'))
                    return false;

                // Check local part doesn't start or end with dot
                if (parts[0].StartsWith(".") || parts[0].EndsWith("."))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // E.164 format validation: +[country code][subscriber number]
            // Total length: 8-16 characters (including +)
            if (!E164PhonePattern().IsMatch(phoneNumber))
                return false;

            // Additional length check
            if (phoneNumber.Length < 8 || phoneNumber.Length > 16)
                return false;

            return true;
        }
    }

    public class ValidationResult
    {
        private readonly List<string> _errors = new();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        public void AddError(string error)
        {
            _errors.Add(error);
        }

        public string GetErrorMessage()
        {
            return string.Join("\n", _errors);
        }
    }
}
