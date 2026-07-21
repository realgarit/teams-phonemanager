using PhoneDesk.Models;

namespace PhoneDesk.Services.Interfaces;

/// <summary>
/// Service for validating user inputs and application state.
/// </summary>
public interface IValidationService
{
    ValidationResult ValidatePrerequisites();
    ValidationResult ValidateVariables(IPhoneManagerVariables variables);
    ValidationResult ValidateHolidayDate(DateTime holidayDate);
    bool IsValidEmail(string email);
    bool IsValidPhoneNumber(string phoneNumber);
}
