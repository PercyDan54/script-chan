using log4net;
using Newtonsoft.Json;
using Osu.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Osu.Api.Enums;

namespace Osu.Api
{
    /// <summary>
    /// Represents an osu!api wrapper
    /// </summary>
    public class OsuApi
    {
        #region Attributes
        /// <summary>
        /// The base osu!api url
        /// </summary>
        private const string OsuUrl = "http://osu.ppy.sh/api/";

        /// <summary>
        /// The cache filename
        /// </summary>
        private const string CacheFilename = "osu!api.db";

        /// <summary>
        /// The error message in case the api key is not valid
        /// </summary>
        private const string ErrorMessage = "{\"error\":\"Please provide a valid API key.\"}";

        /// <summary>
        /// The logger
        /// </summary>
        private static ILog log = LogManager.GetLogger("osu!api");

        /// <summary>
        /// If the osu!api is initialized or not
        /// </summary>
        private static bool initialized;

        /// <summary>
        /// The cache
        /// </summary>
        private static Cache cache;

        /// <summary>
        /// The osu!api key
        /// </summary>
        private static string key;

        /// <summary>
        /// If the api key is valid
        /// </summary>
        private static bool valid;
        #endregion

        #region Properties
        /// <summary>
        /// Key property
        /// </summary>
        public static string Key
        {
            get => key;
            set
            {
                // Change the key
                key = value;

                // Set valid to false
                valid = false;

                // Change the cached key
                cache["key"] = key;
            }
        }

        /// <summary>
        /// Valid property
        /// </summary>
        public static bool Valid => valid;

        #endregion

        #region Beatmap
        /// <summary>
        /// Gets a beatmap from the osu! api
        /// </summary>
        /// <param name="id">the beatmap id</param>
        /// <param name="refresh">if we should refresh the cached data</param>
        /// <returns>the beatmap, or null if none exists</returns>
        public static async Task<OsuBeatmap> GetBeatmap(long id, bool refresh)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            // If we don't have to refresh and the cache contains the map id
            if (!refresh && cache.TryGetValue(id.ToString(), out object value))
                return JsonConvert.DeserializeObject<OsuBeatmap>((string)value);
            else
            {
                log.Info("Requesting beatmap " + id);

                // Create a request builder
                RequestBuilder builder = new RequestBuilder(OsuUrl + "get_beatmaps", key);

                // Add the beatmap id
                builder.Add("b", id.ToString());

                // Get the response
                var json = await builder.GetAsync();

                // If the json is not valid
                if (!IsValid(json))
                    // Null
                    return null;
                try
                {
                    // Get the beatmap list
                    List<OsuBeatmap> beatmaps = JsonConvert.DeserializeObject<List<OsuBeatmap>>(json);

                    // No beatmap
                    if (beatmaps == null || beatmaps.Count == 0)
                        // Null
                        return null;
                    // Else
                    else
                    {
                        // Get the first beatmap
                        OsuBeatmap beatmap = beatmaps[0];

                        // Store the beatmap in the cache
                        cache[id.ToString()] = JsonConvert.SerializeObject(beatmap);

                        // Return the beatmap
                        return beatmap;
                    }
                }
                // If the json could not be parsed
                catch (JsonSerializationException e)
                {
                    log.Error("Serialization exception trying to retrieve beatmap " + id, e);

                    // Null
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a beatmap set from the osu! api
        /// </summary>
        /// <param name="id">the beatmap set id</param>
        /// <returns>the beatmap set</returns>
        public async Task<OsuBeatmap[]> GetBeatmapSet(long id, bool refresh)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            // If we don't have to refresh and the value is in the cache
            if (!refresh && cache.TryGetValue(id.ToString(), out object value))
                // Return it
                return JsonConvert.DeserializeObject<OsuBeatmap[]>((string)value);
            // Else
            else
            {
                log.Info("Requesting beatmap set " + id);

                // Create a request builder
                RequestBuilder builder = new RequestBuilder(OsuUrl + "get_beatmaps", key);

                // Add the beatmap set id
                builder.Add("s", id.ToString());

                // Get the response
                var json = await builder.GetAsync();

                // If the json is not valid
                if (!IsValid(json))
                    // Null
                    return null;

                try
                {
                    // Get the beatmaps
                    OsuBeatmap[] beatmaps = JsonConvert.DeserializeObject<OsuBeatmap[]>(json);

                    // Store them in the cache
                    cache[id.ToString()] = JsonConvert.SerializeObject(beatmaps);

                    // Return them
                    return beatmaps;
                }
                // If the json could not be parsed
                catch (JsonSerializationException e)
                {
                    log.Error("Serialization exception trying to retrieve beatmap set " + id, e);

                    // Null
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a user beatmaps from the osu! api
        /// </summary>
        /// <param name="id">the user id</param>
        /// <returns>the user beatmaps</returns>
        public async Task<OsuBeatmap[]> GetUserBeatmap(long id, bool refresh)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            // If we don't have to refresh and the value is in the cache
            if (!refresh && cache.TryGetValue(id.ToString(), out var value))
                // Return it
                return JsonConvert.DeserializeObject<OsuBeatmap[]>((string)value);
            // Else
            else
            {
                log.Info("Requesting beatmaps of the user " + id);

                // Create a request builder
                RequestBuilder builder = new RequestBuilder(OsuUrl + "get_beatmaps", key);

                // Add the user id
                builder.Add("u", id.ToString());

                // Get the response
                var json = await builder.GetAsync();

                // If the json is not valid
                if (!IsValid(json))
                    // Null
                    return null;

                try
                {
                    // Get the beatmaps
                    OsuBeatmap[] beatmaps = JsonConvert.DeserializeObject<OsuBeatmap[]>(json);

                    // Store them in the cache
                    cache[id.ToString()] = JsonConvert.SerializeObject(json);

                    // Return them
                    return beatmaps;
                }
                // If the json could not be parsed
                catch (JsonSerializationException e)
                {
                    log.Error("Serialization exception trying to retrieve beatmaps of the user " + id, e);

                    // Null
                    return null;
                }
            }
        }
        #endregion

        #region User
        /// <summary>
        /// Returns an user from the given parameters
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="mode">the mode</param>
        /// <param name="refresh">if we should update the cached values</param>
        /// <returns>a list of users</returns>
        public static async Task<OsuUser> GetUser(string username, OsuMode mode, bool refresh)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            // If we don't have to refresh and the cache contains the key
            if (!refresh && cache.TryGetValue("u" + username + mode.ToString(), out var value))
                // Return it
                return JsonConvert.DeserializeObject<OsuUser>((string)value);
            // Else
            else
            {
                log.Info("Requesting user " + username + " for mode " + mode);

                // Create a request builder
                RequestBuilder builder = new RequestBuilder(OsuUrl + "get_user", key);

                // Add the username
                builder.Add("u", username);

                // Add the mode
                switch (mode.ToString())
                {
                    case "Standard":
                        builder.Add("m", "0");
                        break;
                    case "Taiko":
                        builder.Add("m", "1");
                        break;
                    case "CTB":
                        builder.Add("m", "2");
                        break;
                    case "Mania":
                        builder.Add("m", "3");
                        break;

                }

                // Add the type of id
                builder.Add("type", "string");

                // Get the response
                var json = await builder.GetAsync();

                // If the json is not valid
                if (!IsValid(json))
                    // Null
                    return null;

                try
                {
                    // Deserialize the json array and return it
                    List<OsuUser> users = JsonConvert.DeserializeObject<List<OsuUser>>(json);

                    // If we don't have any user
                    if (users == null || users.Count == 0)
                        // Null
                        return null;
                    // Else
                    else
                    {
                        System.Console.WriteLine("ok2");
                        // Get the first user
                        OsuUser user = users[0];

                        // Cache it
                        cache["u" + username + mode.ToString()] = JsonConvert.SerializeObject(user);

                        // Return it
                        return user;
                    }
                }
                // If the json could not be parsed
                catch (JsonSerializationException e)
                {
                    log.Error("Serialization exception when trying to retrieve user " + username + " for mode " + mode, e);

                    // Null
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns an user from the given parameters
        /// </summary>
        /// <param name="id">the player id</param>
        /// <param name="mode">the played mode</param>
        /// <param name="refresh">if we should refresh the cached value</param>
        /// <returns></returns>
        public static async Task<OsuUser> GetUser(long id, OsuMode mode, bool refresh)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            // Temp value

            // If we don't have to refresh and the cache contains the key
            if (!refresh && cache.TryGetValue("u" + id.ToString() + mode.ToString(), out var value))
                // Return it
                return JsonConvert.DeserializeObject<OsuUser>((string)value);
            // Else
            else
            {
                log.Info("Requesting user " + id + " for mode " + mode);

                // Create a request builder
                RequestBuilder builder = new RequestBuilder(OsuUrl + "get_user", key);

                // Add the username
                builder.Add("u", id.ToString());

                // Add the mode
                int modeNumber = (int)mode;
                builder.Add("m", modeNumber.ToString());

                // Add the type of id
                builder.Add("type", "id");

                // Get the response
                var json = await builder.GetAsync();

                // If the json is not valid
                if (!IsValid(json))
                    // Null
                    return null;

                try
                {
                    // Deserialize the json array and return it
                    List<OsuUser> users = JsonConvert.DeserializeObject<List<OsuUser>>(json);

                    // If we don't have any user
                    if (users == null || users.Count == 0)
                        // Null
                        return null;
                    // Else
                    else
                    {
                        // Get the first user
                        OsuUser user = users[0];

                        // Cache it
                        cache["u" + id.ToString() + mode.ToString()] = JsonConvert.SerializeObject(user);

                        // Return it
                        return user;
                    }
                }
                // If the json could not be parsed
                catch (JsonSerializationException e)
                {
                    log.Error("Serialization exception when trying to retrieve user " + id + " for mode " + mode, e);

                    // Null
                    return null;
                }
            }
        }
        #endregion

        #region Multiplayer
        /// <summary>
        /// Returns a single room from the given id
        /// </summary>
        /// <param name="id">the multiplayer room id</param>
        /// <returns>the room, or null if none was found</returns>
        public static async Task<OsuRoom> GetRoom(long id)
        {
            // Null if not yet initialized
            if (!initialized)
                return null;

            log.Info("Requesting room " + id);

            // Create a request builder
            RequestBuilder builder = new RequestBuilder(OsuUrl + "get_match", key);

            // Add the id
            builder.Add("mp", id.ToString());

            // Get the response
            var json = await builder.GetAsync();

            // If the json is not valid
            if (!IsValid(json))
                // Null
                return null;

            try
            {
                // Deserialize the json array and return it
                return JsonConvert.DeserializeObject<OsuRoom>(json);
            }
            // If the json could not be parsed
            catch (JsonSerializationException e)
            {
                log.Error("Serialization exception when trying to retrieve room " + id, e);

                // Null
                return null;
            }
        }
        #endregion

        #region Misc
        /// <summary>
        /// Initializes the osu!api
        /// </summary>
        public static void Initialize()
        {
            // Create cache
            cache = Cache.GetCache(CacheFilename);

            // Get key
            key = cache.Get("key", "");

            // Set valid to false
            valid = false;

            // Set initialized to true
            initialized = true;
        }

        /// <summary>
        /// Checks if the given json is valid
        /// </summary>
        /// <param name="json">the json</param>
        /// <returns>if the json is valid or not</returns>
        private static bool IsValid(string json)
        {
            if (json == "TIMEOUT")
                return false;

            return !json.Equals(ErrorMessage);
        }

        /// <summary>
        /// Checks if the provided api key is valid or not
        /// </summary>
        /// <returns>true/false</returns>
        public static async Task<ReturnCodeAPI> CheckKey()
        {
            log.Info("Checking validity of the api key");

            // Create a request builder
            RequestBuilder builder = new RequestBuilder(OsuUrl + "get_user", key);

            // Add the username
            builder.Add("u", "BanchoBot");

            // Add the mode
            builder.Add("m", "std");

            // Add the type of id
            builder.Add("type", "string");

            try
            {
                // Get the response
                string json = await builder.GetAsync();

                // If the API timed out
                if(json == "TIMEOUT")
                {
                    return ReturnCodeAPI.TIMEOUT;
                }
                // If the JSON is not valid
                else if (!IsValid(json))
                    // Null
                    return ReturnCodeAPI.KO;

                // Set valid to true
                valid = true;

                // Valid
                return ReturnCodeAPI.OK;
            }
            // If an exception occurs (Typically Error 401)
            catch (Exception)
            {
                // Set valid to false
                valid = false;

                // Not valid
                return ReturnCodeAPI.KO;
            }
        }
        #endregion
    }
}