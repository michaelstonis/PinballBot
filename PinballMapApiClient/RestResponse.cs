using System;
using System.Collections;
using System.Collections.Generic;

namespace PinballMapApiClient
{
    public class RestResponse
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string Response { get; set; }

        public int StatusCode { get; set; }

        public IList<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();
    }

    public class RestResponse<T, TError> : RestResponse
    {
        public T Result { get; set; }

        public bool IsSuccessfulWithResult
        {
            get
            {
                return this.IsSuccess && this.Result != null;
            }
        }

        public bool IsSuccessfulWithResultAndNotEmpty
        {
            get
            {
                if (this.Result is IEnumerable)
                {
                    var hasItem = false;

                    var collection = (IEnumerable<object>)this.Result;
                    using (var enumer = collection.GetEnumerator())
                    {
                        enumer.MoveNext();
                        hasItem = enumer.Current != null;
                    }

                    return this.IsSuccess && this.Result != null && hasItem;
                }
                else
                    return this.IsSuccess && this.Result != null;
            }
        }

        public TError Error { get; set; }

        public bool HasError { get { return this.Error != null; } }

        public RestErrorResponse<TError> ToErrorRespose()
        {
            return new RestErrorResponse<TError>()
            {
                IsSuccess = this.IsSuccess,
                Message = this.Message,
                Response = this.Response,
                StatusCode = this.StatusCode,
                Headers = this.Headers,
                Error = this.Error
            };
        }
    }

    public class RestErrorResponse<TError> : RestResponse
    {
        public TError Error { get; set; }
    }
}