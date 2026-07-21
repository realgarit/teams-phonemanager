namespace PhoneDesk.Services
{
    /// <summary>
    /// Accumulates validation errors. Pure value object — lives in the Domain layer so both
    /// Domain rules and Application/Presentation validators can produce and consume it.
    /// </summary>
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
