using Hangfire.Example.Mail;
using ServiceStack;

namespace Hangfire.Example.ServiceInterface;

public class DummyMailService(IMailJobRegistrationService mailService) : Service
{
    public void Post(PostSendEMailRequest _)
    {
        mailService.SendMail();
    }
}

public class PostSendEMailRequest : IPost, IReturnVoid
{
}