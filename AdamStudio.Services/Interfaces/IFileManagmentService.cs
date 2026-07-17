using System;
using System.Threading.Tasks;
using System.Xml;

namespace AdamStudio.Services.Interfaces
{
    public interface IFileManagmentService : IDisposable
    {

        public Task WriteAsync(string path, string file);

        public Task<string> ReadTextAsStringAsync(string path);

        public Task<byte[]> ReadTextAsByteArray(string path);

        public XmlTextReader ReadTextAsXml(byte[] xml);

    }
}
