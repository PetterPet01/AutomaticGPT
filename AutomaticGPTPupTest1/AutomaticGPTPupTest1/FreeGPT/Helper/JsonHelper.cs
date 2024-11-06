using Newtonsoft.Json;

namespace PetterPet.FreeGPT.Helper
{
    internal static class JsonHelper
    {
        internal static T? ParseJson<T>(
            string value,
            MissingMemberHandling missingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling nullValueHandling = NullValueHandling.Ignore
        )
        {
            if (value == string.Empty)
                throw new ArgumentException("Json is empty");

            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = missingMemberHandling,
                NullValueHandling = nullValueHandling
            };
            T? result = JsonConvert.DeserializeObject<T>(value, settings);
            return result;
        }

        internal static bool ParseJsonWithException<T>(
            string value,
            out T? result,
            out Exception? exception,
            MissingMemberHandling missingMemberHandling = MissingMemberHandling.Error
        )
        {
            if (value == string.Empty)
            {
                exception = new ArgumentException("Json is empty");
                result = default;
                return false;
            }

            bool success = true;
            Exception? e = null;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    success = false;
                    e = args.ErrorContext.Error;
                },
                MissingMemberHandling = missingMemberHandling,
            };
            result = JsonConvert.DeserializeObject<T>(value, settings);
            exception = e;
            return success;
        }

        internal static bool TryParseJson<T>(
            string value,
            out T? result,
            MissingMemberHandling missingMemberHandling = MissingMemberHandling.Error
        )
        {
            if (value == string.Empty)
                throw new ArgumentException("Json is empty");

            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    success = false;
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = missingMemberHandling
            };
            result = JsonConvert.DeserializeObject<T>(value, settings);
            return success;
        }

        internal static string SerializeObject<T>(
            T? value,
            MissingMemberHandling missingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling nullValueHandling = NullValueHandling.Ignore
        )
        {
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = missingMemberHandling,
                NullValueHandling = nullValueHandling
            };
            string result = JsonConvert.SerializeObject(value, settings);
            return result;
        }
    }
}
