using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PinballMapApiClient
{
    public class PinballMapApiClient : IDisposable
    {
        public const string ApiBaseAddress = "https://www.pinballmap.com";

        public HttpClient Client { get; }

        public bool IncludeFullRequest { get; set; }

        public bool IncludeFullResponse { get; set; }

        public bool AwaitSerialization { get; set; } = true;

        public JsonSerializerSettings SerializerSettings { get; set; } =
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                    new IsoDateTimeConverter
                    {
                        DateTimeStyles = DateTimeStyles.AssumeUniversal
                    },
                },
            };

        public int UploadBufferSize { get; set; } = 512;

        public string LogName { get; set; } = "PinballMapApiClient";

        public Action<string> Log { get; set; }

        private JsonSerializer Serializer { get; set; }

        private bool DisposeClient { get; }

        private const string DEFAULT_MEDIA_TYPE = "application/json";

        const int columnHeaderSize = 85, columnSize = 35;

        public PinballMapApiClient(HttpClient client = null, string overrideBaseAddress = null)
        {
            if (client == null)
            {
                this.Client = new HttpClient();
                this.DisposeClient = true;
            }
            else
                this.Client = client;

            this.Client.BaseAddress = new Uri(!string.IsNullOrEmpty(overrideBaseAddress) ? overrideBaseAddress : ApiBaseAddress);
        }

        public PinballMapApiClient(HttpClientHandler clientHandler, string overrideBaseAddress = null)
        {
            if (clientHandler == default(HttpClientHandler))
                throw new ArgumentNullException(nameof(clientHandler));

            this.Client = new HttpClient(clientHandler);

            this.Client.BaseAddress = new Uri(!string.IsNullOrEmpty(overrideBaseAddress) ? overrideBaseAddress : ApiBaseAddress);

            this.DisposeClient = true;
        }

        ///
        public Task<RestResponse<LocationList, JToken>> GetLocationsClosestByAddress(string address, bool sendAllWithinDistance = false, int? maxDistance = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"api/v1/locations/closest_by_address.json?address={address}");

            if(sendAllWithinDistance)
            {
                queryBuilder.Append($"&send_all_within_distance={sendAllWithinDistance}");
            }

            if(maxDistance.HasValue)
            {
                queryBuilder.Append($"&max_distance={maxDistance}");
            }

            return GetJsonAsync<LocationList>(queryBuilder.ToString(), cancellationToken: cancellationToken);
        }


        public Task<RestResponse<RegionList, JToken>> GetAllRegionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetJsonAsync<RegionList>("api/v1/regions.json", cancellationToken: cancellationToken);
        }

        private Task<RestResponse<T, JToken>> GetJsonAsync<T>(string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendRequestAsync<T>(null, HttpMethod.Get, requestUri, headers, cancellationToken);
        }

        private Task<RestResponse<T, JToken>> PostJsonAsync<T>(object o, string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendRequestAsync<T>(o, HttpMethod.Post, requestUri, headers, cancellationToken);
        }

        private Task<RestResponse<T, JToken>> PutJsonAsync<T>(object o, string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendRequestAsync<T>(o, HttpMethod.Put, requestUri, headers, cancellationToken);
        }

        private Task<RestResponse<T, JToken>> PatchJsonAsync<T>(object o, string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendRequestAsync<T>(o, new HttpMethod("Patch"), requestUri, headers, cancellationToken);
        }

        private Task<RestResponse<T, JToken>> DeleteAsync<T>(string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendRequestAsync<T>(null, HttpMethod.Delete, requestUri, headers, cancellationToken);
        }

        private Task<RestResponse<T, JToken>> SendRequestAsync<T>(object o, HttpMethod httpMethod, string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Serializer = JsonSerializer.CreateDefault(this.SerializerSettings);

            var content = new JsonStreamContent(o, true, UploadBufferSize, this.Serializer);

            return SendRequestAsync<T>(content, httpMethod, requestUri, headers, cancellationToken);
        }

        private async Task<RestResponse<T, JToken>> SendRequestAsync<T>(HttpContent content, HttpMethod httpMethod, string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = requestUri;
            if (!Uri.IsWellFormedUriString(requestUri, UriKind.Absolute))
                url = String.Concat(this.Client.BaseAddress.ToString().TrimEnd('/'), "/", requestUri).TrimStart('/');

            var logBuilder = default(StringBuilder);
            var watch = default(Stopwatch);
            var start = default(DateTimeOffset?);

            if (this.Log != null)
            {
                logBuilder = new StringBuilder();
                watch = new Stopwatch();
                start = DateTimeOffset.Now;

                watch.Start();

                logBuilder.AppendLine($"[{this.LogName.PadRight(75, '-'),-75} Start]");

                logBuilder.AppendLine($" {"Base Address",-25}\t{this.Client.BaseAddress}");
                logBuilder.AppendLine($" {"Request Uri",-25}\t{requestUri}");
                logBuilder.AppendLine($" {"Full Url",-25}\t{url}");
                logBuilder.AppendLine($" {"Method",-25}\t{httpMethod.Method}");
            }

            var restResponse = new RestResponse<T, JToken>() { Result = default(T) };
            try
            {
                using (var requestMessage = new HttpRequestMessage())
                {
                    if (headers != null && headers.Count > 0)
                    {
                        for (var i = 0; i < headers.Count; i++)
                        {
                            requestMessage.Headers.Add(headers[i].Key, headers[i].Value);
                        }
                    }

                    requestMessage.Method = httpMethod;
                    requestMessage.RequestUri = new Uri(url);
                    requestMessage.Content = content;

                    if (this.Log != null)
                    {
                        if (this.IncludeFullRequest)
                        {
                            foreach (var header in this.Client.DefaultRequestHeaders)
                                logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");

                            foreach (var header in requestMessage.Headers)
                                logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");

                            if (requestMessage.Content != null)
                            {
                                foreach (var header in requestMessage.Content.Headers)
                                    logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");
                            }

                            if (content != null)
                                logBuilder.AppendLine($" {"Request Content",-25}\t{await content.ReadAsStringAsync()}");
                        }
                    }

                    using (var responseMessage = await this.Client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false))
                    {
                        if (this.Log != null)
                        {
                            logBuilder.AppendLine($" {"Response Received",-25}\t{DateTimeOffset.Now} ({watch.ElapsedMilliseconds}ms)");
                            watch.Restart();
                        }

                        restResponse.Message = responseMessage.ReasonPhrase;
                        restResponse.StatusCode = (int)responseMessage.StatusCode;
                        restResponse.IsSuccess = responseMessage.IsSuccessStatusCode;

                        using (var stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            if (this.IncludeFullResponse)
                            {
                                using (var contentStream = new StreamReader(stream))
                                {
                                    restResponse.Response = await contentStream.ReadToEndAsync().ConfigureAwait(false);
                                }
                            }

                            foreach (var header in responseMessage.Headers)
                                restResponse.Headers.Add(new KeyValuePair<string, string>(header.Key, String.Join(", ", header.Value)));

                            foreach (var header in responseMessage.Content.Headers)
                                restResponse.Headers.Add(new KeyValuePair<string, string>(header.Key, String.Join(", ", header.Value)));

                            if (this.Log != null)
                            {
                                logBuilder.AppendLine($" {"Read",-25}\t{DateTimeOffset.Now} ({ watch.ElapsedMilliseconds}ms)");

                                watch.Restart();

                                foreach (var header in restResponse.Headers)
                                    logBuilder.AppendLine($" {"Response Header",-25}\t{header.Key} - {header.Value}");

                                logBuilder.AppendLine($" {"Response Message",-25}\t{restResponse.Message}");
                                logBuilder.AppendLine($" {"Response Status",-25}\t{restResponse.StatusCode}");

                                if (this.IncludeFullResponse)
                                    logBuilder.AppendLine($" {"Response Content",-25}\t{restResponse.Response}");

                                logBuilder.AppendLine($" {"Deserialization Start",-25}\t{DateTimeOffset.Now}");
                                watch.Restart();
                            }

                            if (!this.IncludeFullResponse && this.Serializer == null)
                                this.Serializer = JsonSerializer.CreateDefault(this.SerializerSettings);

                            if (restResponse.IsSuccess && this.IncludeFullResponse)
                            {
                                if (this.AwaitSerialization)
                                    restResponse.Result = await Task.Run(() => JsonConvert.DeserializeObject<T>(restResponse.Response, this.SerializerSettings)).ConfigureAwait(false);
                                else
                                    restResponse.Result = JsonConvert.DeserializeObject<T>(restResponse.Response, this.SerializerSettings);
                            }
                            else if (restResponse.IsSuccess && !this.IncludeFullResponse)
                            {
                                using (var streamReader = new StreamReader(stream))
                                using (var jsonTextReader = new JsonTextReader(streamReader))
                                {
                                    if (this.AwaitSerialization)
                                        restResponse.Result = await Task.Run(() => this.Serializer.Deserialize<T>(jsonTextReader)).ConfigureAwait(false);
                                    else
                                        restResponse.Result = this.Serializer.Deserialize<T>(jsonTextReader);
                                }
                            }
                            if (!restResponse.IsSuccess && this.IncludeFullResponse)
                            {
                                if (this.AwaitSerialization)
                                    restResponse.Error = await Task.Run(() => JsonConvert.DeserializeObject<JToken>(restResponse.Response, this.SerializerSettings)).ConfigureAwait(false);
                                else
                                    restResponse.Error = JsonConvert.DeserializeObject<JToken>(restResponse.Response, this.SerializerSettings);
                            }
                            else if (!restResponse.IsSuccess && !this.IncludeFullResponse)
                            {
                                using (var streamReader = new StreamReader(stream))
                                using (var jsonTextReader = new JsonTextReader(streamReader))
                                {
                                    if (this.AwaitSerialization)
                                        restResponse.Error = await Task.Run(() => this.Serializer.Deserialize<JToken>(jsonTextReader)).ConfigureAwait(false);
                                    else
                                        restResponse.Error = this.Serializer.Deserialize<JToken>(jsonTextReader);
                                }
                            }

                            logBuilder?.AppendLine($" {"Deserialization End",-25}\t{DateTimeOffset.Now} ({ watch.ElapsedMilliseconds}ms)");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                restResponse.IsSuccess = false;
                restResponse.StatusCode = 999;
                restResponse.Message = $"\n{ex.Message}\n{ex.StackTrace}";

                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    restResponse.Message += $"\n\n{innerException.Message}\n{innerException.StackTrace}";

                    innerException = innerException.InnerException;
                }

                logBuilder?.AppendLine($" {"ERROR!!!",-25}\t{restResponse.Message}");
            }
            finally
            {
                watch?.Stop();

                var now = DateTimeOffset.Now;
                logBuilder?.AppendLine($" {"Finished",-25}\t{now} ({(now - start).Value.TotalMilliseconds}ms)");
                logBuilder?.AppendLine($"[{this.LogName.PadRight(75, '-'),-75} End]");

                this.Log?.Invoke(logBuilder.ToString());
            }

            return restResponse;
        }

        private string GetHeaderValue(string key, IEnumerable<string> values)
        {
            var result = String.Join(", ", values);
            if (String.Equals(key, "authorization", StringComparison.OrdinalIgnoreCase) && !String.IsNullOrWhiteSpace(key))
                result = $"{result.Substring(0, (int)Math.Floor(result.Length / 2.0))}...";

            return result;
        }

        public virtual async Task<RestResponse<byte[], JToken>> DownloadAsync(string requestUri, IList<KeyValuePair<string, string>> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var url = requestUri;
            if (!Uri.IsWellFormedUriString(requestUri, UriKind.Absolute))
                url = String.Concat(this.Client.BaseAddress.ToString().TrimEnd('/'), "/", requestUri).TrimStart('/');

            var logBuilder = default(StringBuilder);
            var watch = default(Stopwatch);
            var start = default(DateTimeOffset?);

            if (this.Log != null)
            {
                logBuilder = new StringBuilder();
                watch = new Stopwatch();
                start = DateTimeOffset.Now;

                watch.Start();

                logBuilder.AppendLine($"[{this.LogName.PadRight(75, '-'),-75} Start]");

                logBuilder.AppendLine($" {"Base Address",-25}\t{this.Client.BaseAddress}");
                logBuilder.AppendLine($" {"Request Uri",-25}\t{requestUri}");
                logBuilder.AppendLine($" {"Full Url",-25}\t{url}");
                logBuilder.AppendLine($" {"Method",-25}\t{HttpMethod.Get}");
            }

            var restResponse = new RestResponse<byte[], JToken>() { Result = default(byte[]) };
            try
            {
                using (var requestMessage = new HttpRequestMessage())
                {
                    if (headers != null && headers.Count > 0)
                    {
                        for (var i = 0; i < headers.Count; i++)
                        {
                            requestMessage.Headers.Add(headers[i].Key, headers[i].Value);
                        }
                    }

                    requestMessage.Method = HttpMethod.Get;
                    requestMessage.RequestUri = new Uri(url);
                    //requestMessage.Content = content;

                    if (this.Log != null)
                    {
                        if (this.IncludeFullRequest)
                        {
                            foreach (var header in this.Client.DefaultRequestHeaders)
                                logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");

                            foreach (var header in requestMessage.Headers)
                                logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");

                            if (requestMessage.Content != null)
                            {
                                foreach (var header in requestMessage.Content.Headers)
                                    logBuilder.AppendLine($" {"Request Header",-25}\t{header.Key} - {String.Join(", ", GetHeaderValue(header.Key, header.Value))}");
                            }

                            //if (content != null)
                            //    logBuilder.AppendLine($" {"Request Content",-25}\t{await content.ReadAsStringAsync()}");
                        }
                    }

                    using (var responseMessage = await this.Client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false))
                    {
                        if (this.Log != null)
                        {
                            logBuilder.AppendLine($" {"Response Received",-25}\t{DateTimeOffset.Now} ({watch.ElapsedMilliseconds}ms)");
                            watch.Restart();
                        }

                        restResponse.Message = responseMessage.ReasonPhrase;
                        restResponse.StatusCode = (int)responseMessage.StatusCode;
                        restResponse.IsSuccess = responseMessage.IsSuccessStatusCode;

                        restResponse.Result = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                        //if (this.IncludeFullResponse)
                        //{
                        //    using (var contentStream = new StreamReader(stream))
                        //    {
                        //        restResponse.Response = await contentStream.ReadToEndAsync().ConfigureAwait(false);
                        //    }
                        //}

                        foreach (var header in responseMessage.Headers)
                            restResponse.Headers.Add(new KeyValuePair<string, string>(header.Key, String.Join(", ", header.Value)));

                        foreach (var header in responseMessage.Content.Headers)
                            restResponse.Headers.Add(new KeyValuePair<string, string>(header.Key, String.Join(", ", header.Value)));

                        if (this.Log != null)
                        {
                            logBuilder.AppendLine($" {"Read",-25}\t{DateTimeOffset.Now} ({ watch.ElapsedMilliseconds}ms)");

                            watch.Restart();

                            foreach (var header in restResponse.Headers)
                                logBuilder.AppendLine($" {"Response Header",-25}\t{header.Key} - {header.Value}");

                            logBuilder.AppendLine($" {"Response Message",-25}\t{restResponse.Message}");
                            logBuilder.AppendLine($" {"Response Status",-25}\t{restResponse.StatusCode}");

                            if (this.IncludeFullResponse)
                                logBuilder.AppendLine($" {"Response Content",-25}\t{restResponse.Response}");

                            //logBuilder.AppendLine($" {"Deserialization Start",-25}\t{DateTimeOffset.Now}");
                            watch.Restart();
                        }

                        //if (!this.IncludeFullResponse && this.Serializer == null)
                        //    this.Serializer = JsonSerializer.CreateDefault(this.SerializerSettings);

                        logBuilder?.AppendLine($" {"Deserialization End",-25}\t{DateTimeOffset.Now} ({ watch.ElapsedMilliseconds}ms)");
                    }
                }
            }
            catch (Exception ex)
            {
                restResponse.IsSuccess = false;
                restResponse.StatusCode = 999;
                restResponse.Message = $"\n{ex.Message}\n{ex.StackTrace}";

                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    restResponse.Message += $"\n\n{innerException.Message}\n{innerException.StackTrace}";

                    innerException = innerException.InnerException;
                }

                logBuilder?.AppendLine($" {"ERROR!!!",-25}\t{restResponse.Message}");
            }
            finally
            {
                watch?.Stop();

                var now = DateTimeOffset.Now;
                logBuilder?.AppendLine($" {"Finished",-25}\t{now} ({(now - start).Value.TotalMilliseconds}ms)");
                logBuilder?.AppendLine($"[{this.LogName.PadRight(75, '-'),-75} End]");

                this.Log?.Invoke(logBuilder.ToString());
            }

            return restResponse;
        }

        public void Dispose()
        {
            if (this.DisposeClient && this.Client != null)
                this.Client.Dispose();
        }
    }
}