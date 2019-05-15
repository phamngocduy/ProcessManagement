using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace ProcessManagement.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetNickName(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("NickName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetDefaultAvatar(this IIdentity identity)
        {
            Claim defaultavatar = ((ClaimsIdentity)identity).FindFirst("AvatarDefault");
            return (defaultavatar != null) ? defaultavatar.Value : string.Empty;

        }
        public static string GetAvatar(this IIdentity identity)
        {
            Claim avatar = ((ClaimsIdentity)identity).FindFirst("Avatar");
            // Test for null to avoid issues during local testing
            return (avatar != null) ? avatar.Value : string.Empty;

        }
    }
}