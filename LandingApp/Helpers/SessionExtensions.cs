using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LandingApp.Helpers
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// Сохраняет объект в сессию как JSON-строку.
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session), "Session is not available.");

            var json = JsonConvert.SerializeObject(value, _serializerSettings);
            session.SetString(key, json);
        }

        /// Получает объект из сессии, десериализуя его из JSON-строки.
        public static T? GetObject<T>(this ISession session, string key)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session), "Session is not available.");

            var json = session.GetString(key);
            return string.IsNullOrEmpty(json)
                ? default
                : JsonConvert.DeserializeObject<T>(json, _serializerSettings);
        }

        /// Удаляет объект из сессии.
        public static void RemoveObject(this ISession session, string key)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session), "Session is not available.");

            session.Remove(key);
        }
    }
}
