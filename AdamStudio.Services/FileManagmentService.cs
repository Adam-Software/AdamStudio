using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AdamStudio.Services
{
    public class FileManagmentService : IFileManagmentService
    {
        #region Const

        private const int cBufferSize = 0x4096;

        #endregion

        #region ~

        public FileManagmentService() 
        {
            
            
        }

        #endregion

        #region Public methods

        public XmlTextReader ReadTextAsXml(byte[] xmlByteArray)
        {
            MemoryStream stream = new(xmlByteArray);
            XmlTextReader reader = new(stream);

            return reader;
        }

        public async Task<byte[]> ReadTextAsByteArray(string path)
        {
            using var fs = OpenFileStreamAsync(path);

            var buffer = new byte[fs.Length];
            _ = await fs.ReadAsync(buffer.AsMemory(0, (int)fs.Length));
            return buffer;
        }

        public async Task<string> ReadTextAsStringAsync(string path)
        {
            using FileStream sourceStream = OpenFileStreamAsync(path);

            StringBuilder sb = new();

            byte[] buffer = new byte[0x1000];
            int numRead;

            while ((numRead = await sourceStream.ReadAsync(buffer)) != 0)
            {
                string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                _ = sb.Append(text);
            }

            return sb.ToString();
        }

        public async Task WriteAsync(string path, string file)
        {
            using StreamWriter writer = File.CreateText(path);
            await writer.WriteAsync(file);
        }

        public void Dispose()
        {

        }

        #endregion

        #region Private mehods

        private static FileStream OpenFileStreamAsync(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: cBufferSize, useAsync: true);
        }

        #endregion

    }
}
