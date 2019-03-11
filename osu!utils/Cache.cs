using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Osu.Utils
{
    /// <summary>
    /// Represents a simple cache, stored in a json file
    /// </summary>
    public class Cache
    {
        #region Attributes
        /// <summary>
        /// The list of available caches
        /// </summary>
        protected static Dictionary<string, Cache> caches;

        /// <summary>
        /// The dictionary values
        /// </summary>
        protected Dictionary<string, object> values;

        /// <summary>
        /// The cache path
        /// </summary>
        protected string path;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">the path of the cache</param>
        private Cache(string path)
        {
            values = new Dictionary<string, object>();

            this.path = path;

            if (File.Exists(path))
                ParseFile();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Path property
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;

                ParseFile();
            }
        }

        /// <summary>
        /// Returns a key from the cache, or initialize it with a default value if
        /// it doesn't exist yet
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the default value</param>
        /// <returns></returns>
        public T Get<T>(string key, T value)
        {
            // Old value
            object old;

            // Try to get the value at the given key
            if (values.TryGetValue(key, out old))
                // Return this value
                return (T)old;

            // Add it
            values[key] = value;

            // Return the value given by the user
            return value;
        }

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <typeparam name="T">the object type</typeparam>
        /// <param name="key">the key</param>
        /// <param name="value">the default value</param>
        /// <returns>the object</returns>
        public T GetObject<T>(string key, T value)
        {
            // Old value
            object old;

            // Try to get the value at the given key
            if (values.TryGetValue(key, out old))
                // Return this value
                return ((JObject)old).ToObject<T>();

            // Add it
            values[key] = value;

            // Return the value given by the user
            return value;
        }

        /// <summary>
        /// Returns an array from the cache, or initialize it with a default value if it doesn't exist yet
        /// </summary>
        /// <typeparam name="T">the type of the array</typeparam>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        /// <returns></returns>
        public T GetArray<T>(string key, T value)
        {
            // Old value
            object old;

            // If the value already exists
            if (values.TryGetValue(key, out old))
                // Get the value as a JArray and cast it to a list
                return ((JArray)old).ToObject<T>();

            // Else, store the value
            values[key] = value;

            // Return the value
            return value;
        }

        /// <summary>
        /// [] Property
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the value</returns>
        public object this[string key]
        {
            get
            {
                return values[key];
            }
            set
            {
                values[key] = value;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Saves the cache
        /// </summary>
        private void Save()
        {
            // Serialize the dictionary in a json string
            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            // Write the json string in the file given by the path
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        /// <summary>
        /// Parses the file using the given path
        /// </summary>
        private void ParseFile()
        {
            // Read the file at the given path
            string json = File.ReadAllText(path, Encoding.UTF8);

            // File is empty
            if (string.IsNullOrEmpty(json))
                // Stop here
                return;

            // Deserialize the json string in a dictionary
            values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Tries to get a value from the cache
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        /// <returns>true or false</returns>
        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Returns the cache with the given name
        /// </summary>
        /// <param name="name">the name of the cache</param>
        /// <returns>the cache</returns>
        public static Cache GetCache(string name)
        {
            if (caches == null)
                caches = new Dictionary<string, Cache>();

            if (!caches.ContainsKey(name))
                caches[name] = new Cache(name);

            return caches[name];
        }

        /// <summary>
        /// Saves all the created caches
        /// </summary>
        public static void SaveAll()
        {
            foreach (Cache cache in caches.Values)
                cache.Save();
        }
        #endregion
    }
}
