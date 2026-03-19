using Avalonia.Controls;
using Avalonia.Controls.Templates;
using teams_phonemanager.Services;

namespace teams_phonemanager.Converters
{
    public class PageTemplateSelector : IDataTemplate
    {
        private static readonly Dictionary<string, string> PageKeyMap = BuildPageKeyMap();

        private static Dictionary<string, string> BuildPageKeyMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var page in new[]
            {
                ConstantsService.Pages.Welcome,
                ConstantsService.Pages.GetStarted,
                ConstantsService.Pages.Variables,
                ConstantsService.Pages.M365Groups,
                ConstantsService.Pages.CallQueues,
                ConstantsService.Pages.AutoAttendants,
                ConstantsService.Pages.Holidays,
                ConstantsService.Pages.Documentation,
                ConstantsService.Pages.Wizard,
                ConstantsService.Pages.BulkOperations,
            })
            {
                map[page] = page;
                map[page.Replace(" ", "")] = page;
            }
            return map;
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
            if (item is string pageName && PageKeyMap.TryGetValue(pageName, out var normalizedPage))
            {
                return normalizedPage switch
                {
                    ConstantsService.Pages.Welcome => WelcomeTemplate,
                    ConstantsService.Pages.GetStarted => GetStartedTemplate,
                    ConstantsService.Pages.Variables => VariablesTemplate,
                    ConstantsService.Pages.M365Groups => M365GroupsTemplate,
                    ConstantsService.Pages.CallQueues => CallQueuesTemplate,
                    ConstantsService.Pages.AutoAttendants => AutoAttendantsTemplate,
                    ConstantsService.Pages.Holidays => HolidaysTemplate,
                    ConstantsService.Pages.Documentation => DocumentationTemplate,
                    ConstantsService.Pages.Wizard => WizardTemplate,
                    ConstantsService.Pages.BulkOperations => BulkOperationsTemplate,
                    _ => WelcomeTemplate
                };
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
