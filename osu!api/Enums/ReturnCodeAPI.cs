using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Api.Enums
{
    /// <summary>
    /// Represents the return type we can get from the API
    /// </summary>
    public enum ReturnCodeAPI
    {
        /// <summary>
        /// If the API returns the JSON properly
        /// </summary>
        OK,

        /// <summary>
        /// If there is a problem with the JSON or the key is incorrect
        /// </summary>
        KO,

        /// <summary>
        /// If the request has timed out
        /// </summary>
        TIMEOUT
    }
}
