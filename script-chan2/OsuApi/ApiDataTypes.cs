using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace script_chan2.OsuApi
{
    class ApiBeatmap
    {
        public string approved { get; set; }
        public string submit_date { get; set; }
        public string approved_date { get; set; }
        public string last_update { get; set; }
        public string artist { get; set; }
        public string beatmap_id { get; set; }
        public string beatmapset_id { get; set; }
        public string bpm { get; set; }
        public string creator { get; set; }
        public string creator_id { get; set; }
        public string difficultyrating { get; set; }
        public string diff_aim { get; set; }
        public string diff_speed { get; set; }
        public string diff_size { get; set; }
        public string diff_overall { get; set; }
        public string diff_approach { get; set; }
        public string diff_drain { get; set; }
        public string hit_length { get; set; }
        public string source { get; set; }
        public string genre_id { get; set; }
        public string language_id { get; set; }
        public string title { get; set; }
        public string total_length { get; set; }
        public string version { get; set; }
        public string file_md5 { get; set; }
        public string mode { get; set; }
        public string tags { get; set; }
        public string favourite_count { get; set; }
        public string rating { get; set; }
        public string playcount { get; set; }
        public string passcount { get; set; }
        public string count_normal { get; set; }
        public string count_slider { get; set; }
        public string count_spinner { get; set; }
        public string max_combo { get; set; }
        public string download_unavailable { get; set; }
        public string audio_unavailable { get; set; }
    }

    class PlayerEvent
    {
        public string display_html { get; set; }
        public string beatmap_id { get; set; }
        public string beatmapset_id { get; set; }
        public string date { get; set; }
        public string epicfactor { get; set; }
    }

    class ApiPlayer
    {
        public string user_id { get; set; }
        public string username { get; set; }
        public string join_date { get; set; }
        public string count300 { get; set; }
        public string count100 { get; set; }
        public string count50 { get; set; }
        public string playcount { get; set; }
        public string ranked_score { get; set; }
        public string total_score { get; set; }
        public string pp_rank { get; set; }
        public string level { get; set; }
        public string pp_raw { get; set; }
        public string accuracy { get; set; }
        public string count_rank_ss { get; set; }
        public string count_rank_ssh { get; set; }
        public string count_rank_s { get; set; }
        public string count_rank_sh { get; set; }
        public string count_rank_a { get; set; }
        public string country { get; set; }
        public string total_seconds_played { get; set; }
        public string pp_country_rank { get; set; }
        public List<PlayerEvent> events { get; set; }
    }

    public class ApiScore
    {
        public string slot { get; set; }
        public string team { get; set; }
        public string user_id { get; set; }
        public string score { get; set; }
        public string maxcombo { get; set; }
        public string rank { get; set; }
        public string count50 { get; set; }
        public string count100 { get; set; }
        public string count300 { get; set; }
        public string countmiss { get; set; }
        public string countgeki { get; set; }
        public string countkatu { get; set; }
        public string perfect { get; set; }
        public string pass { get; set; }
        public string enabled_mods { get; set; }
    }

    public class ApiDetail
    {
        public string match_id { get; set; }
        public string name { get; set; }
        public string start_time { get; set; }
        public object end_time { get; set; }
    }

    public class ApiGame
    {
        public string game_id { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string beatmap_id { get; set; }
        public string play_mode { get; set; }
        public string match_type { get; set; }
        public string scoring_type { get; set; }
        public string team_type { get; set; }
        public string mods { get; set; }
        public List<ApiScore> scores { get; set; }
    }

    public class ApiMatch
    {
        public ApiDetail match { get; set; }
        public List<ApiGame> games { get; set; }
    }



    public class WebsiteMatchDetail
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class WebsiteMatchCovers
    {
        public string cover { get; set; }
        [JsonProperty("cover@2x")]
        public string cover2x { get; set; }
        public string card { get; set; }
        [JsonProperty("card@2x")]
        public string card2x { get; set; }
        public string list { get; set; }
        [JsonProperty("list@2x")]
        public string list2x { get; set; }
        public string slimcover { get; set; }
        [JsonProperty("slimcover@2x")]
        public string slimcover2x{ get; set; } 
    }

    public class WebsiteMatchBeatmapset
    {
        public string artist { get; set; }
        public string artist_unicode { get; set; }
        public WebsiteMatchCovers covers { get; set; }
        public string creator { get; set; }
        public int favourite_count { get; set; }
        public int id { get; set; }
        public int play_count { get; set; }
        public string preview_url { get; set; }
        public string source { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string title_unicode { get; set; }
        public int user_id { get; set; }
        public bool video { get; set; }
    }

    public class WebsiteMatchBeatmap
    {
        public double difficulty_rating { get; set; }
        public int id { get; set; }
        public string mode { get; set; }
        public string version { get; set; }
        public WebsiteMatchBeatmapset beatmapset { get; set; }
    }

    public class WebsiteMatchStatistics
    {
        public int count_50 { get; set; }
        public int count_100 { get; set; }
        public int count_300 { get; set; }
        public int count_geki { get; set; }
        public int count_katu { get; set; }
        public int count_miss { get; set; }
    }

    public class WebsiteMatchMatch
    {
        public int slot { get; set; }
        public string team { get; set; }
        public int pass { get; set; }
    }

    public class WebsiteMatchScore
    {
        public object id { get; set; }
        public object best_id { get; set; }
        public int user_id { get; set; }
        public double accuracy { get; set; }
        public List<string> mods { get; set; }
        public int score { get; set; }
        public int max_combo { get; set; }
        public int perfect { get; set; }
        public WebsiteMatchStatistics statistics { get; set; }
        public object pp { get; set; }
        public object rank { get; set; }
        public object created_at { get; set; }
        public WebsiteMatchMatch match { get; set; }
    }

    public class WebsiteMatchGame
    {
        public int id { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string mode { get; set; }
        public int mode_int { get; set; }
        public string scoring_type { get; set; }
        public string team_type { get; set; }
        public List<string> mods { get; set; }
        public WebsiteMatchBeatmap beatmap { get; set; }
        public List<WebsiteMatchScore> scores { get; set; }
    }

    public class WebsiteMatchEvent
    {
        public int id { get; set; }
        public WebsiteMatchDetail detail { get; set; }
        public DateTime timestamp { get; set; }
        public int? user_id { get; set; }
        public WebsiteMatchGame game { get; set; }
    }

    public class WebsiteMatchCountry
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class WebsiteMatchUser
    {
        public string avatar_url { get; set; }
        public string country_code { get; set; }
        public string default_group { get; set; }
        public int id { get; set; }
        public bool is_active { get; set; }
        public bool is_bot { get; set; }
        public bool is_online { get; set; }
        public bool is_supporter { get; set; }
        public DateTime? last_visit { get; set; }
        public bool pm_friends_only { get; set; }
        public object profile_colour { get; set; }
        public string username { get; set; }
        public WebsiteMatchCountry country { get; set; }
    }

    public class WebsiteMatch
    {
        public List<WebsiteMatchEvent> events { get; set; }
        public List<WebsiteMatchUser> users { get; set; }
        public int latest_event_id { get; set; }
        public int? current_game_id { get; set; }
    }


}
