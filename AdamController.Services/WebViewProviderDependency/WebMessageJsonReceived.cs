﻿using System;
using System.Text.Json.Serialization;

namespace AdamController.Services.WebViewProviderDependency
{
    public class WebMessageJsonReceived : EventArgs
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }
    }
}
