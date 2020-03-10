using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
