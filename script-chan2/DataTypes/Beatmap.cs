namespace script_chan2.DataTypes
{
    public class Beatmap
    {
        public int Id { get; set; }

        public int SetId { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string Version { get; set; }

        public string Creator { get; set; }

        public decimal BPM { get; set; }

        public decimal AR { get; set; }

        public decimal CS { get; set; }
    }
}
