using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Osu.Api
{
    /// <summary>
    /// Represents a request builder
    /// </summary>
    class RequestBuilder
    {
        #region Classes
        /// <summary>
        /// Represents a url parameter
        /// </summary>
        private class Parameter
        {
            /// <summary>
            /// The parameter key
            /// </summary>
            public readonly string key;

            /// <summary>
            /// The parameter value
            /// </summary>
            public readonly string value;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="key">the parameter key</param>
            /// <param name="value">the parameter value</param>
            public Parameter(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }
        #endregion

        #region Attributes
        /// <summary>
        /// The base url
        /// </summary>
        private string base_url;

        /// <summary>
        /// The osu!api key
        /// </summary>
        private string api_key;

        /// <summary>
        /// The list of parameters
        /// </summary>
        private List<Parameter> parameters;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="base_url">the base url</param>
        /// <param name="key">the osu!api key</param>
        public RequestBuilder(string baseUrl, string key)
        {
            this.base_url = baseUrl;
            this.api_key = key;
            parameters = new List<Parameter>();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds and return the complete url
        /// </summary>
        /// <returns></returns>
        private string MakeUrl()
        {
            string url = base_url + "?k=" + (string.IsNullOrEmpty(api_key) ? "somerandomkey" : api_key);

            foreach (Parameter parameter in parameters)
                url += "&" + parameter.key + "=" + parameter.value;

            return url;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a parameter
        /// </summary>
        /// <param name="key">the key of the parameter</param>
        /// <param name="value">the value of the parameter</param>
        public void Add(string key, string value)
        {
            parameters.Add(new Parameter(key, value));
        }

        /// <summary>
        /// Sends the request and return the response as a string
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            // Create the request
            WebRequest request = WebRequest.Create(MakeUrl());

            // Get the response
            WebResponse response = request.GetResponse();

            // Create a stream on the response
            StreamReader reader = new StreamReader(response.GetResponseStream());

            // Read and return the stream content
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Get a response asynchronously
        /// </summary>
        /// <returns>the response</returns>
        public async Task<string> GetAsync()
        {
            WebRequest request = WebRequest.Create(MakeUrl());

            WebResponse response = await request.GetResponseAsync();

            StreamReader reader = new StreamReader(response.GetResponseStream());

            return await reader.ReadToEndAsync();
        }
        #endregion
    }
}
