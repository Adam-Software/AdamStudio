using AdamStudio.Services.Interfaces;
using AdamController.WebApi.Client;
using AdamController.WebApi.Client.v1.RequestModel;
using AdamController.WebApi.Client.v1.ResponseModel;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AdamStudio.Services.WebApiServiceDependency;

namespace AdamStudio.Services
{
    public class WebApiService : IWebApiService
    {

        #region Var

        private readonly BaseApi mBaseApi;

        #endregion

        #region ~

        public WebApiService(IServiceProvider serviceProvider)
        {
            WebApiSettings settings = serviceProvider.GetService<IServiceSettings>().WebApiSettings;

            Uri defaultUri = new($"http://{settings.Ip}:{settings.Port}");
            mBaseApi = new BaseApi(defaultUri, settings.Login, settings.Password);
        }

        #endregion

        #region Public methods

        public Task<ExtendedCommandExecuteResult> GetPythonBinDir()
        {
            return mBaseApi.PythonCommand.GetPythonBinDir();
        }

        public Task<ExtendedCommandExecuteResult> GetPythonVersion()
        {
            return mBaseApi.PythonCommand.GetVersion();
        }

        public Task<ExtendedCommandExecuteResult> GetPythonWorkDir()
        {
            return mBaseApi.PythonCommand.GetPythonWorkDir();
        }

        public Task<ExtendedCommandExecuteResult> PythonExecuteAsync(PythonCommandModel command)
        {
            return mBaseApi.PythonCommand.ExecuteAsync(command);
        }

        public Task<ExtendedCommandExecuteResult> StopPythonExecute()
        {
            return mBaseApi.PythonCommand.StopExecuteAsync();
        }

        public Task<ExtendedCommandExecuteResult> MoveToZeroPosition()
        {
            return mBaseApi.AdamSdk.MoveToZeroPosition();
        }

        public void Dispose()
        {
            mBaseApi.Dispose();
        }

        #endregion
    }
}
