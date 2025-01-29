namespace Hangfire.Example.Mail;

public class MailJobRegistrationService :IMailJobRegistrationService
{
    public void SendMail()
    {
        BackgroundJob.Enqueue<IMailDeliveryService>(service => service.SendMail());
    }
}