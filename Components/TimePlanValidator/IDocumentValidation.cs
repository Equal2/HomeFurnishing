using TimePlanValidator.ViewModels;

namespace TimePlanValidator
{
    public interface IDocumentValidation
    {
        bool ValidateDocument(DocumentUniqueId Args, string Type, string UserName, out string Msg, out bool Continue);
        bool ValidateDocumentLine(DocumentUniqueId Args, string UserName, out string Msg, out bool Continue);
    }
}
