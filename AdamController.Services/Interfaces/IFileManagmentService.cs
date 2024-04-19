﻿using System;
using System.Threading.Tasks;

namespace AdamController.Services.Interfaces
{
    public interface IFileManagmentService : IDisposable
    {
        #region Public methods

        public Task WriteAsync(string path, string file);

        public Task<string> ReadTextAsStringAsync(string path);

        public Task<byte[]> ReadTextAsByteArray(string path);

        #endregion
    }
}
