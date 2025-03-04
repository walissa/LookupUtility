﻿using BizTalkComponents.Utilities.LookupUtility.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BizTalkComponents.Utilities.LookupUtility
{
    public class LookupUtilityService
    {
        private ILookupRepository _lookupRepository;
        private readonly Dictionary<string, Dictionary<string, string>> _lookupValues = new Dictionary<string, Dictionary<string, string>>();
        private const string DEFAULT_KEY = "default";

        public LookupUtilityService(ILookupRepository lookupRepository)
        {
            if(lookupRepository == null)
            {
                throw new InvalidOperationException("LookupRepository is not set.");
            }

            _lookupRepository = lookupRepository;
        }

        public string GetValue(string list, string key, string defaultValue, int maxAgeSeconds)
        {
            return GetValue(list, key, defaultValue, maxAgeSeconds == -1 ? default(TimeSpan) : new TimeSpan(0, 0, maxAgeSeconds));
        }

        public string GetValue(string list, string key, string defaultValue, TimeSpan maxAge = default(TimeSpan))
        {
            var dict = GetList(list, maxAge);
            string val;
            if (!dict.TryGetValue(key, out val))
            {
                return defaultValue;
            }

            return val;
        }

        public string GetValue(string list, string key, int maxAgeSeconds, bool throwIfNotExists = false, bool allowDefaults = false)
        {
            return GetValue(list, key, throwIfNotExists, allowDefaults, maxAgeSeconds == -1 ? default(TimeSpan) : new TimeSpan(0, 0, maxAgeSeconds));
        }

        public string GetValue(string list, string key, bool throwIfNotExists = false, bool allowDefaults = false, TimeSpan maxAge = default(TimeSpan))
        {
            var dict = GetList(list, maxAge);
            string val;
            if (!dict.TryGetValue(key, out val))
            {
                if (throwIfNotExists)
                {
                    throw new ArgumentException(string.Format("The specified property {0} does not exist in list {1}", key, list));
                }
                string defaultValue;
                if (allowDefaults && dict.TryGetValue(DEFAULT_KEY, out defaultValue))
                {
                    return defaultValue;
                }

                return null;
            }

            return val;
        }

        public string GetValue(string list, string key, string defaultValue)
        {
            var dict = GetList(list, default(TimeSpan));
            string val;
            if (!dict.TryGetValue(key, out val))
            {
                
                if (dict.TryGetValue(DEFAULT_KEY, out defaultValue))
                {
                    return defaultValue;
                }
                throw new ArgumentException(string.Format("The specified property {0} does not exist in list {1}", key, list));
            }

            return val;
        }


        private Dictionary<string, string> GetList(string list, TimeSpan maxAge = default(TimeSpan))
        {
            Trace.WriteLine($"Getting list {list}");
            var dict = new Dictionary<string, string>();

            if (!_lookupValues.TryGetValue(list, out dict))
            {
                dict = _lookupRepository.LoadList(list, maxAge);

                if (dict == null)
                {
                    throw new ArgumentException("The list {0} does not exist.", list);
                }

                _lookupValues.Add(list, dict);
            }

            return dict;
        }
    }
}
