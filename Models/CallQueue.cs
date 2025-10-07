using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class CallQueue : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _identity = string.Empty;

        [ObservableProperty]
        private string _routingMethod = string.Empty;

        [ObservableProperty]
        private int _agentAlertTime;

        [ObservableProperty]
        private bool _isSelected;

        public CallQueue()
        {
        }

        public CallQueue(string name, string identity, string routingMethod, int agentAlertTime)
        {
            Name = name;
            Identity = identity;
            RoutingMethod = routingMethod;
            AgentAlertTime = agentAlertTime;
        }
    }
}
