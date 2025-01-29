using Funq;
using Hangfire.Example.Mail;
using Hangfire.Example.Mail.Delivery;
using Hangfire.Example.ServiceInterface;
using ServiceStack.Configuration;
using ServiceStack.Text;

namespace Hangfire.Example;

public class AppHost : AppHostBase
{
    public AppHost(IConfiguration configuration) : base("Example", typeof(DummyMailService).Assembly)
    {
        AppSettings = new MultiAppSettingsBuilder()
            .AddAppSettings()
            .AddNetCore(configuration)
            .Build();
    }

    public override void Configure(Container container)
    {
        // Servicestack.Text
        JsConfig.TreatEnumAsInteger = true;
        JsConfig.DateHandler = DateHandler.ISO8601;
        JsConfig.SkipDateTimeConversion = true;

        SetupMail(container);

        SetConfig(new HostConfig
        {
            AddRedirectParamsToQueryString = true
        });
    }

    private void SetupMail(Container container)
    {
        container.RegisterAutoWiredAs<MailJobRegistrationService, IMailJobRegistrationService>()
            .ReusedWithin(ReuseScope.None);
        container.RegisterAutoWiredAs<MailDeliveryService, IMailDeliveryService>().ReusedWithin(ReuseScope.None);
    }
}