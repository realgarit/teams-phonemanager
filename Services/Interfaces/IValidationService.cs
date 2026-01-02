using teams_phonemanager.Models;

namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for validating user inputs and application state.
/// </summary>
public interface IValidationService
{
    ValidationResult ValidatePrerequisites();
    ValidationResult ValidateVariables(PhoneManagerVariables variables);
    ValidationResult ValidateHolidayDate(DateTime holidayDate);
    bool IsValidEmail(string email);
    bool IsValidPhoneNumber(string phoneNumber);
}
