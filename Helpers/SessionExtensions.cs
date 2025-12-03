using Microsoft.AspNetCore.Http;

namespace Cinema.Helpers
{
    public static class SessionExtensions
    {
        private const string UserIdKey = "UserId";

        public static void SetUserId(this ISession session, int userId)
        {
            session.SetInt32(UserIdKey, userId);
        }

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32(UserIdKey);
        }

        public static void RemoveUserId(this ISession session)
        {
            session.Remove(UserIdKey);
        }
    }
}
