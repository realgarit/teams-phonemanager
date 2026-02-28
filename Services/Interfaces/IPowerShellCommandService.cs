using teams_phonemanager.Models;

namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for building PowerShell command strings.
/// </summary>
public interface IPowerShellCommandService
{
    // Common commands
    string GetCheckModulesCommand();
    string GetConnectTeamsCommand();
    string GetConnectGraphCommand();
    string GetConnectGraphWithTokenCommand(string accessToken);
    string GetDisconnectTeamsCommand();
    string GetDisconnectGraphCommand();
    string GetImportAudioFileCommand(string filePath);
    string GetAssignLicenseCommand(string userId, string skuId);

    // M365 Groups
    string GetCreateM365GroupCommand(string groupName);
    string GetRemoveM365GroupCommand(string groupId, string groupName);
    string GetRetrieveM365GroupsCommand();
    string GetM365GroupIdCommand(string groupName);

    // Call Queue operations
    string GetRetrieveCallQueuesCommand();
    string GetCreateCallQueueCommand(PhoneManagerVariables variables);
    string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId, PhoneManagerVariables? variables = null);
    string GetAssociateResourceAccountWithCallQueueCommand(string resourceAccountUpn, string callQueueName);
    string GetValidateCallQueueResourceAccountCommand(string racqUpn);
    string GetCreateCallTargetCommand(string racqUpn);
    string GetRemoveCallQueueCommand(string callQueueName);
    string GetRemoveResourceAccountCommand(string upn);

    // Auto Attendant operations
    string GetRetrieveAutoAttendantsCommand();
    string GetCreateAutoAttendantCommand(PhoneManagerVariables variables);
    string GetCreateSimpleAutoAttendantCommand(PhoneManagerVariables variables);
    string GetCreateSimpleAutoAttendantCommand(string aaName, string languageId, string timeZoneId);
    string GetVerifyAutoAttendantCommand(string aaDisplayName);
    string GetAttachHolidayToAutoAttendantCommand(string holidayName, string aaDisplayName, string holidayGreetingPrompt);
    string GetAssociateResourceAccountWithAutoAttendantCommand(string resourceAccountUpn, string autoAttendantName);
    string GetAssignPhoneNumberToAutoAttendantCommand(string upn, string phoneNumber, string phoneNumberType);
    string GetCreateDefaultCallFlowCommand(PhoneManagerVariables variables);
    string GetCreateAfterHoursCallFlowCommand(PhoneManagerVariables variables);
    string GetCreateAfterHoursScheduleCommand(PhoneManagerVariables variables);
    string GetCreateCallHandlingAssociationCommand();
    string GetRemoveAutoAttendantCommand(string autoAttendantName);
    string GetRemoveScheduleCommand(string scheduleName);

    // Holiday operations
    string GetCreateHolidayCommand(string holidayName, DateTime holidayDate);
    string GetCreateHolidaySeriesCommand(string holidayName, List<DateTime> holidayDates);
    string GetCreateHolidaySeriesFromEntriesCommand(string holidayName, List<HolidayEntry> holidayEntries);

    // Resource Account operations
    string GetRetrieveResourceAccountsCommand();
    string GetRetrieveAutoAttendantResourceAccountsCommand();
    string GetCreateResourceAccountCommand(PhoneManagerVariables variables);
    string GetCreateResourceAccountCommand(string upn, string displayName, string appId);
    string GetCreateAutoAttendantResourceAccountCommand(PhoneManagerVariables variables);
    string GetCreateAutoAttendantResourceAccountCommand(string upn, string displayName, string appId);
    string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation);
    string GetUpdateAutoAttendantResourceAccountUsageLocationCommand(string upn, string usageLocation);
    string GetAssignAutoAttendantLicenseCommand(string userId, string skuId);
}
