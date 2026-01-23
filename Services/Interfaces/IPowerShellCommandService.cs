using teams_phonemanager.Models;

namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for building PowerShell command strings.
/// </summary>
public interface IPowerShellCommandService
{
    string GetCheckModulesCommand();
    string GetConnectTeamsCommand();
    string GetConnectGraphCommand();
    string GetConnectGraphWithTokenCommand(string accessToken);
    string GetDisconnectTeamsCommand();
    string GetDisconnectGraphCommand();
    string GetCreateM365GroupCommand(string groupName);
    string GetCreateCallQueueCommand(PhoneManagerVariables variables);
    string GetCreateAutoAttendantCommand(PhoneManagerVariables variables);
    string GetCreateHolidayCommand(string holidayName, DateTime holidayDate);
    string GetCreateHolidaySeriesCommand(string holidayName, List<DateTime> holidayDates);
    string GetVerifyAutoAttendantCommand(string aaDisplayName);
    string GetAttachHolidayToAutoAttendantCommand(string holidayName, string aaDisplayName, string holidayGreetingPrompt);
    string GetRetrieveM365GroupsCommand();
    string GetRetrieveResourceAccountsCommand();
    string GetRetrieveCallQueuesCommand();
    string GetRetrieveAutoAttendantsCommand();
    string GetRetrieveAutoAttendantResourceAccountsCommand();
    string GetCreateResourceAccountCommand(PhoneManagerVariables variables);
    string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation);
    string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId, PhoneManagerVariables? variables = null);
    string GetImportAudioFileCommand(string filePath);
    string GetAssociateResourceAccountWithCallQueueCommand(string resourceAccountUpn, string callQueueName);
    string GetM365GroupIdCommand(string groupName);
    string GetAssignLicenseCommand(string userId, string skuId);
}
