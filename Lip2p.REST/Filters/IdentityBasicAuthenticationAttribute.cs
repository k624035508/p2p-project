using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Lip2p.Common;
using Lip2p.Linq2SQL;

namespace Lip2p.REST.Filters
{
    // 文档 http://www.asp.net/web-api/overview/security/authentication-filters
    /// <summary>
    /// 指定验证功能，RequestContext.Principal.Identity.Name 取得用户名
    /// </summary>
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var context = new Lip2pDataContext();

            var user = context.dt_users.SingleOrDefault(u => u.user_name == userName);

            if (user == null || DESEncrypt.Encrypt(password, user.salt) != user.password)
            {
                // No user with userName/password exists.
                return null;
            }

            return new GenericPrincipal(new GenericIdentity(userName), null);
        }
    }
}