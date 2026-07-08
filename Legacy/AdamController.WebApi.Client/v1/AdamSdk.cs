using AdamController.WebApi.Client.Common;
using AdamController.WebApi.Client.v1.ResponseModel;
using System.Net.Http;

namespace AdamController.WebApi.Client.v1
{
    #region Interface

    public interface IAdamSdk
    {
        public Task<ExtendedCommandExecuteResult> MoveToZeroPosition();
    }

    #endregion

    internal class AdamSdk : IAdamSdk
    {
        #region Const
        
        private const string cApiPath = "AdamSdk";

        #endregion

        #region Var

        private readonly ApiClient mClient;

        #endregion

        #region ~

        internal AdamSdk(ApiClient client)
        {
            mClient = client;
        }

        #endregion

        #region Public methods

        public Task<ExtendedCommandExecuteResult> MoveToZeroPosition()
        {
            Task<HttpResponseMessage> responseMessage = mClient.Get($"{cApiPath}/MoveToZeroPosition");
            Task<ExtendedCommandExecuteResult> result = responseMessage.ToExtendedCommandResultAsync();
            return result;
        }

        #endregion
    }
}
