using AdamController.WebApi.Client.v1.ResponseModel;
using System.Net.Http;
using System.Text.Json;

namespace AdamController.WebApi.Client.Common
{
    public static class Extension
    {
        private static readonly JsonSerializerOptions mJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        internal static Task<ExtendedCommandExecuteResult> ToExtendedCommandResultAsync(this Task<HttpResponseMessage> response)
        {
            if (response == null)
                return Task.FromResult(new ExtendedCommandExecuteResult());

            Task<ExtendedCommandExecuteResult> result = Task.Run(async () =>
            {
                var responseMessage = await response;
                var jsonString = await responseMessage.Content.ReadAsStringAsync();
                var result = jsonString.ToExtendedCommandResult();
                return result;
            });

            return result;
        }

        public static ExtendedCommandExecuteResult ToExtendedCommandResult(this string jsonString)
        {
            ExtendedCommandExecuteResult result = new();

            try
            {
                ExtendedCommandExecuteResult? deleserialize = JsonSerializer.Deserialize<ExtendedCommandExecuteResult>(jsonString, mJsonSerializerOptions);

                if (deleserialize != null)
                    result = deleserialize;

                return result;
            }
            catch
            {
                return result;
            }
        }
    }
}
