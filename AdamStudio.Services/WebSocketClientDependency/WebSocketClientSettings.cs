using System;

namespace AdamStudio.Services.WebSocketClientDependency
{
    public class WebSocketClientSettings
    {
        public WebSocketClientSettings(Uri uri) 
        { 
            Uri = uri;
        }

        public Uri Uri { get; set; }
    }
}
