namespace ShortUrl.Common.Utility.ToolsHelpers
{
    public class ApiPipelineValidationMessage
    {
        public ApiPipelineValidationMessage()
        {
            Messages = new List<AppMessage>();
        }

        public List<AppMessage> Messages { get; set; }

        public void Add(AppMessage message)
        {
            Messages.Add(message);
        }

        public void AddInfo(string message)
        {
            Messages.Add(new AppMessage(message, EnmMessageTypes.Info));
        }

        public void AddWarning(string message)
        {
            Messages.Add(new AppMessage(message, EnmMessageTypes.Warning));
        }

        public void AddOk(string message)
        {
            Messages.Add(new AppMessage(message, EnmMessageTypes.Ok));
        }

        public void AddError(string message)
        {
            Messages.Add(new AppMessage(message));
        }

        public void AddConfirmation(string message)
        {
            Messages.Add(new AppMessage(message, EnmMessageTypes.Confirmation));
        }

        public bool Any => Messages.Any();

        public bool AnyError => Messages.Any(p => p.Type == EnmMessageTypes.Error);
        public bool AnyConfirmation => Messages.Any(p => p.Type == EnmMessageTypes.Confirmation);
        public List<string> GetErrors() => Messages.Where(p => p.Type == EnmMessageTypes.Error).Select(p => p.Message).ToList();

        public void ClearMessages()
        {
            Messages.Clear();
        }
    }

    public class AppMessage
    {
        public AppMessage()
        {

        }

        public AppMessage(string message, EnmMessageTypes type = EnmMessageTypes.Error)
        {
            Type = type;
            Message = message;
        }

        public EnmMessageTypes Type { get; set; }

        public string Message { get; set; }
    }

    public enum EnmMessageTypes
    {
        Ok = 1,
        Error = 2,
        Warning = 3,
        Info = 4,
        Confirmation = 5
    }
}
