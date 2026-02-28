using Avalonia.Controls;
using Avalonia.Controls.Templates;
using teams_phonemanager.Services;

namespace teams_phonemanager.Converters
{
    public class PageTemplateSelector : IDataTemplate
    {
        public PageTemplateSelector()
        {
            // PageTemplateSelector is created by XAML, so we cannot use constructor injection here
        }

        public required IDataTemplate WelcomeTemplate { get; set; }
        public required IDataTemplate GetStartedTemplate { get; set; }
        public required IDataTemplate VariablesTemplate { get; set; }
        public required IDataTemplate M365GroupsTemplate { get; set; }
        public required IDataTemplate CallQueuesTemplate { get; set; }
        public required IDataTemplate AutoAttendantsTemplate { get; set; }
        public required IDataTemplate HolidaysTemplate { get; set; }
        public required IDataTemplate DocumentationTemplate { get; set; }
        public required IDataTemplate WizardTemplate { get; set; }
        public required IDataTemplate BulkOperationsTemplate { get; set; }

        public IDataTemplate SelectTemplate(object? item, Control? container)
        {
            if (item is string pageName)
            {
                var normalizedPageName = pageName.Replace(" ", "");
                var normalizedWelcome = ConstantsService.Pages.Welcome.Replace(" ", "");
                var normalizedGetStarted = ConstantsService.Pages.GetStarted.Replace(" ", "");
                var normalizedVariables = ConstantsService.Pages.Variables.Replace(" ", "");
                var normalizedM365Groups = ConstantsService.Pages.M365Groups.Replace(" ", "");
                var normalizedCallQueues = ConstantsService.Pages.CallQueues.Replace(" ", "");
                var normalizedAutoAttendants = ConstantsService.Pages.AutoAttendants.Replace(" ", "");
                var normalizedHolidays = ConstantsService.Pages.Holidays.Replace(" ", "");
                var normalizedDocumentation = ConstantsService.Pages.Documentation.Replace(" ", "");
                var normalizedWizard = ConstantsService.Pages.Wizard.Replace(" ", "");
                var normalizedBulkOperations = ConstantsService.Pages.BulkOperations.Replace(" ", "");
                
                IDataTemplate template = normalizedPageName switch
                {
                    var name when name == normalizedWelcome => WelcomeTemplate,
                    var name when name == normalizedGetStarted => GetStartedTemplate,
                    var name when name == normalizedVariables => VariablesTemplate,
                    var name when name == normalizedM365Groups => M365GroupsTemplate,
                    var name when name == normalizedCallQueues => CallQueuesTemplate,
                    var name when name == normalizedAutoAttendants => AutoAttendantsTemplate,
                    var name when name == normalizedHolidays => HolidaysTemplate,
                    var name when name == normalizedDocumentation => DocumentationTemplate,
                    var name when name == normalizedWizard => WizardTemplate,
                    var name when name == normalizedBulkOperations => BulkOperationsTemplate,
                    _ => WelcomeTemplate
                };

                return template;
            }

            return WelcomeTemplate;
        }

        public Control? Build(object? param)
        {
            var template = SelectTemplate(param, null);
            return template?.Build(param);
        }

        public bool Match(object? data)
        {
            return data is string;
        }
    }
}
