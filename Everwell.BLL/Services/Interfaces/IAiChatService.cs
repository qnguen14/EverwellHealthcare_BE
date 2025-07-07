namespace Everwell.BLL.Services.Interfaces;

public interface IAiChatService
{
    Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default);
} 