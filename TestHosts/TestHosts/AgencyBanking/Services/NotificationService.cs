using System;
using System.Threading.Tasks;

namespace TestHosts.AgencyBanking.Services;

public interface INotificationService {
    Task Send(string recipient,
              string message);
}

public class NotificationService : INotificationService {
    public async Task Send(string recipient,
                           string message)
    {
        Console.WriteLine($"NOTIFICATION => {recipient} => {message}");

        await Task.CompletedTask;
    }
}