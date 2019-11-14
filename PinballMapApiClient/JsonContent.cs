using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;

namespace PinballMapApiClient
{
    public class JsonStreamContent : HttpContent
    {
        private const string _gzipEncoding = "gzip";

        private static readonly MediaTypeHeaderValue _jsonMediaTypeHeaderValue = new MediaTypeHeaderValue("application/json");
        private static readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        private readonly JsonSerializer _serializer;
        private readonly object _value;
        private readonly bool _isGzip;
        private readonly int _bufferSize;

        public JsonStreamContent(object value, bool isGzip, int bufferSize, JsonSerializer serializer = null)
        {
            _value = value;

            _isGzip = isGzip;

            _bufferSize = bufferSize;

            _serializer = serializer ?? new JsonSerializer();

            this.Headers.ContentType = _jsonMediaTypeHeaderValue;

            if (_isGzip)
                this.Headers.ContentEncoding.Add(_gzipEncoding);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(() =>
            {
                if (_isGzip)
                {
                    using (var gzip = new GZipStream(stream, CompressionLevel.Fastest, true))
                    using (var sw = new StreamWriter(gzip, _encoding, _bufferSize, true))
                    using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
                    {
                        _serializer.Serialize(jtw, _value);
                        jtw.Flush();
                    }
                }
                else
                {
                    using (var sw = new StreamWriter(stream, _encoding, _bufferSize, true))
                    using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
                    {
                        _serializer.Serialize(jtw, _value);
                        jtw.Flush();
                    }
                }
            });
        }
    }
}