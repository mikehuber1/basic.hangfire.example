using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.Extensions.Primitives;

namespace Hangfire.Example.Filters;

public class HangfireHttpBasicAuthenticationFilter : IDashboardAuthorizationFilter
{
    private const string AuthenticationScheme = "Basic";
    private readonly string _password;
    private readonly string _user;

    public HangfireHttpBasicAuthenticationFilter(string user, string password)
    {
        _user = user;
        _password = password;
    }

    // https://gist.github.com/ndc/a1cc8e2515e5e0d941a884fc6a6267f5
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers["Authorization"];

        if (MissingAuthorizationHeader(header))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        var authValues = AuthenticationHeaderValue.Parse(header);
#pragma warning restore CS8604 // Possible null reference argument.

        if (NotBasicAuthentication(authValues))
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        var tokens = ExtractAuthenticationTokens(authValues);

        if (tokens.AreInvalid())
        {
            SetChallengeResponse(httpContext);
            return false;
        }

        if (tokens.CredentialsMatch(_user, _password)) return true;

        SetChallengeResponse(httpContext);
        return false;
    }

    private static bool MissingAuthorizationHeader(StringValues header)
    {
        return string.IsNullOrWhiteSpace(header);
    }

    private static BasicAuthenticationTokens ExtractAuthenticationTokens(AuthenticationHeaderValue authValues)
    {
        if (authValues.Parameter == null)
            return new BasicAuthenticationTokens(new[] { string.Empty, string.Empty });

        var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
        var parts = parameter.Split(':');
        return new BasicAuthenticationTokens(parts);
    }

    private static bool NotBasicAuthentication(AuthenticationHeaderValue authValues)
    {
        return !AuthenticationScheme.Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase);
    }

    private static void SetChallengeResponse(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
    }

    private sealed class BasicAuthenticationTokens
    {
        private readonly string[] _tokens;

        public BasicAuthenticationTokens(string[] tokens)
        {
            _tokens = tokens;
        }

        private string Username => _tokens[0];
        private string Password => _tokens[1];

        public bool AreInvalid()
        {
            return ContainsTwoTokens() && ValidTokenValue(Username) && ValidTokenValue(Password);
        }

        public bool CredentialsMatch(string user, string pass)
        {
            return Username.Equals(user) && Password.Equals(pass);
        }

        private static bool ValidTokenValue(string token)
        {
            return string.IsNullOrWhiteSpace(token);
        }

        private bool ContainsTwoTokens()
        {
            return _tokens.Length == 2;
        }
    }
}