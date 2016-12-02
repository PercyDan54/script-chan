using Osu.Scores;

namespace Osu.Ircbot
{
    /// <summary>
    /// Represents a simple irc handler
    /// </summary>
    public interface IrcHandler
    {
        /// <summary>
        /// Registers the irc bot in the handler
        /// </summary>
        /// <param name="client">the bot</param>
        void RegisterBot(OsuIrcBot bot);

        /// <summary>
        /// Called when a room is added
        /// </summary>
        /// <param name="room"></param>
        void OnAddRoom(Room room);

        /// <summary>
        /// Called when a room is updated
        /// </summary>
        /// <param name="room"></param>
        void OnUpdateRoom(Room room);

        /// <summary>
        /// Called when a room is deleted
        /// </summary>
        void OnDeleteRoom(Room room);

        /// <summary>
        /// Called when a room is changing map
        /// </summary>
        void OnChangeMapRoom(Room room, long map_id, Beatmap beatmap);

        /// <summary>
        /// Called on initialisation to join rooms if we got disconnected
        /// </summary>
        /// <param name="room"></param>
        void onReconnectionRoom();
    }
}
