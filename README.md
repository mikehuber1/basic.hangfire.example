# basic.hangfire.example

Example project for the integration of hangfire into a single solution.

The hangfire data storage is ms-sql. This could (should?) be changed to redis.

**How it works**

- A Job can be registered into the hangfire queue by using `IMailJobRegistrationService`
- After registration, hangfire server calls the method implemented here:`IMailDeliveryService` (you can see the registration in `MailJobRegistrationService`)
- Access hangfire dashboard [http://localhost:5001/dashboard/](http://localhost:5001/dashboard/)  admin/mypass-

**Setup:**
 - Add correct servername, username, password to  connection-string in "appsettings.json" -> "HangfireConnection"
 - Test with request [http://localhost:5001/ui/PostSendEMailRequest](http://localhost:5001/ui/PostSendEMailRequest)
 - You can see a successful job in hangfire dashboard

**Be aware:**
- The hangfire server does **not operate in the origin request context**! A specific implementation of `ContainerJobActivator` is necessary to allow the hangfire server to use the same dependency injection registrations as the request.
- Scaling: If scaling of the service is applied, a second hangfire server will be started. This can be a desired behaviour.