using teams_phonemanager.Models;

namespace teams_phonemanager.Services
{
    public class ValidationService
    {
        private static ValidationService? _instance;

        private ValidationService() { }

        public static ValidationService Instance
        {
            get
            {
                _instance ??= new ValidationService();
                return _instance;
            }
        }

        public ValidationResult ValidatePrerequisites()
        {
            var sessionManager = SessionManager.Instance;
            var result = new ValidationResult();

            if (!sessionManager.ModulesChecked)
            {
                result.AddError("PowerShell modules have not been checked. Please check modules first.");
            }

            if (!sessionManager.TeamsConnected)
            {
                result.AddError("Not connected to Microsoft Teams. Please connect to Teams first.");
            }

            if (!sessionManager.GraphConnected)
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
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber) && 
                   phoneNumber.Length >= 10 && 
                   phoneNumber.StartsWith("+");
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
