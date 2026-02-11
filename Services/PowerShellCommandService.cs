using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services.ScriptBuilders;

namespace teams_phonemanager.Services
{
    public class PowerShellCommandService : IPowerShellCommandService
    {
        private readonly CommonScriptBuilder _commonBuilder;
        private readonly CallQueueScriptBuilder _callQueueBuilder;
        private readonly AutoAttendantScriptBuilder _autoAttendantBuilder;
        private readonly HolidayScriptBuilder _holidayBuilder;
        private readonly ResourceAccountScriptBuilder _resourceAccountBuilder;

        public PowerShellCommandService(
            CommonScriptBuilder commonBuilder,
            CallQueueScriptBuilder callQueueBuilder,
            AutoAttendantScriptBuilder autoAttendantBuilder,
            HolidayScriptBuilder holidayBuilder,
            ResourceAccountScriptBuilder resourceAccountBuilder)
        {
            _commonBuilder = commonBuilder;
            _callQueueBuilder = callQueueBuilder;
            _autoAttendantBuilder = autoAttendantBuilder;
            _holidayBuilder = holidayBuilder;
            _resourceAccountBuilder = resourceAccountBuilder;
        }

        public string GetCheckModulesCommand() => _commonBuilder.GetCheckModulesCommand();
        public string GetConnectTeamsCommand() => _commonBuilder.GetConnectTeamsCommand();
        public string GetConnectGraphCommand() => _commonBuilder.GetConnectGraphWithTokenCommand("");
        public string GetConnectGraphWithTokenCommand(string accessToken) => _commonBuilder.GetConnectGraphWithTokenCommand(accessToken);
        public string GetDisconnectTeamsCommand() => _commonBuilder.GetDisconnectTeamsCommand();
        public string GetDisconnectGraphCommand() => _commonBuilder.GetDisconnectGraphCommand();
        public string GetCreateM365GroupCommand(string groupName) => _commonBuilder.GetCreateM365GroupCommand(groupName);
        public string GetRetrieveM365GroupsCommand() => _commonBuilder.GetRetrieveM365GroupsCommand();
        public string GetM365GroupIdCommand(string groupName) => _commonBuilder.GetM365GroupIdCommand(groupName);
        public string GetImportAudioFileCommand(string filePath) => _commonBuilder.GetImportAudioFileCommand(filePath);
        public string GetAssignLicenseCommand(string userId, string skuId) => _commonBuilder.GetAssignLicenseCommand(userId, skuId);

        public string GetRetrieveCallQueuesCommand() => _callQueueBuilder.GetRetrieveCallQueuesCommand();
        public string GetCreateCallQueueCommand(PhoneManagerVariables variables) => _callQueueBuilder.GetCreateCallQueueCommand(variables);
        public string GetCreateCallQueueCommand(string name, string languageId, string m365GroupId, PhoneManagerVariables? variables = null) => _callQueueBuilder.GetCreateCallQueueCommand(name, languageId, m365GroupId, variables);
        public string GetAssociateResourceAccountWithCallQueueCommand(string resourceAccountUpn, string callQueueName) => _callQueueBuilder.GetAssociateResourceAccountWithCallQueueCommand(resourceAccountUpn, callQueueName);
        public string GetValidateCallQueueResourceAccountCommand(string racqUpn) => _callQueueBuilder.GetValidateCallQueueResourceAccountCommand(racqUpn);
        public string GetCreateCallTargetCommand(string racqUpn) => _callQueueBuilder.GetCreateCallTargetCommand(racqUpn);

        public string GetRetrieveAutoAttendantsCommand() => _autoAttendantBuilder.GetRetrieveAutoAttendantsCommand();
        public string GetCreateAutoAttendantCommand(PhoneManagerVariables variables) => _autoAttendantBuilder.GetCreateAutoAttendantCommand(variables);
        public string GetVerifyAutoAttendantCommand(string aaDisplayName) => _autoAttendantBuilder.GetVerifyAutoAttendantCommand(aaDisplayName);
        public string GetAttachHolidayToAutoAttendantCommand(string holidayName, string aaDisplayName, string holidayGreetingPrompt) => _autoAttendantBuilder.GetAttachHolidayToAutoAttendantCommand(holidayName, aaDisplayName, holidayGreetingPrompt);
        public string GetCreateSimpleAutoAttendantCommand(PhoneManagerVariables variables) => _autoAttendantBuilder.GetCreateSimpleAutoAttendantCommand(variables);
        public string GetAssociateResourceAccountWithAutoAttendantCommand(string resourceAccountUpn, string autoAttendantName) => _autoAttendantBuilder.GetAssociateResourceAccountWithAutoAttendantCommand(resourceAccountUpn, autoAttendantName);
        public string GetAssignPhoneNumberToAutoAttendantCommand(string upn, string phoneNumber, string phoneNumberType) => _autoAttendantBuilder.GetAssignPhoneNumberToAutoAttendantCommand(upn, phoneNumber, phoneNumberType);
        public string GetCreateDefaultCallFlowCommand(PhoneManagerVariables variables) => _autoAttendantBuilder.GetCreateDefaultCallFlowCommand(variables);
        public string GetCreateAfterHoursCallFlowCommand(PhoneManagerVariables variables) => _autoAttendantBuilder.GetCreateAfterHoursCallFlowCommand(variables);
        public string GetCreateAfterHoursScheduleCommand(PhoneManagerVariables variables) => _autoAttendantBuilder.GetCreateAfterHoursScheduleCommand(variables);
        public string GetCreateCallHandlingAssociationCommand() => _autoAttendantBuilder.GetCreateCallHandlingAssociationCommand();

        public string GetCreateHolidayCommand(string holidayName, DateTime holidayDate) => _holidayBuilder.GetCreateHolidayCommand(holidayName, holidayDate);
        public string GetCreateHolidaySeriesCommand(string holidayName, List<DateTime> holidayDates) => _holidayBuilder.GetCreateHolidaySeriesCommand(holidayName, holidayDates);

        public string GetRetrieveResourceAccountsCommand() => _resourceAccountBuilder.GetRetrieveResourceAccountsCommand();
        public string GetCreateResourceAccountCommand(PhoneManagerVariables variables) => _resourceAccountBuilder.GetCreateResourceAccountCommand(variables);
        public string GetUpdateResourceAccountUsageLocationCommand(string upn, string usageLocation) => _resourceAccountBuilder.GetUpdateResourceAccountUsageLocationCommand(upn, usageLocation);
        public string GetRetrieveAutoAttendantResourceAccountsCommand() => _resourceAccountBuilder.GetRetrieveAutoAttendantResourceAccountsCommand();
        public string GetCreateAutoAttendantResourceAccountCommand(PhoneManagerVariables variables) => _resourceAccountBuilder.GetCreateAutoAttendantResourceAccountCommand(variables);
        public string GetUpdateAutoAttendantResourceAccountUsageLocationCommand(string upn, string usageLocation) => _resourceAccountBuilder.GetUpdateAutoAttendantResourceAccountUsageLocationCommand(upn, usageLocation);
        public string GetAssignAutoAttendantLicenseCommand(string userId, string skuId) => _resourceAccountBuilder.GetAssignAutoAttendantLicenseCommand(userId, skuId);
    }
}
