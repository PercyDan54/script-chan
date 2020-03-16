using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class TournamentListItemViewModel : Screen
    {
        #region Constructor
        public TournamentListItemViewModel(Tournament tournament)
        {
            this.tournament = tournament;
        }
        #endregion

        #region Properties
        private Tournament tournament;

        public string Name
        {
            get { return tournament.Name; }
        }
        #endregion

        #region Webhook list
        public BindableCollection<TournamentWebhookListItemViewModel> WebhooksViews
        {
            get
            {
                var list = new BindableCollection<TournamentWebhookListItemViewModel>();
                foreach (var webhook in Database.Database.Webhooks)
                    list.Add(new TournamentWebhookListItemViewModel(tournament, webhook));
                return list;
            }
        }
        #endregion

        #region Edit tournament dialog
        private string editName;
        public string EditName
        {
            get { return editName; }
            set
            {
                if (value != editName)
                {
                    editName = value;
                    NotifyOfPropertyChange(() => EditName);
                    NotifyOfPropertyChange(() => EditTournamentSaveEnabled);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes editGameMode;
        public GameModes EditGameMode
        {
            get { return editGameMode; }
            set
            {
                if (value != editGameMode)
                {
                    editGameMode = value;
                    NotifyOfPropertyChange(() => EditGameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes editTeamMode;
        public TeamModes EditTeamMode
        {
            get { return editTeamMode; }
            set
            {
                if (value != editTeamMode)
                {
                    editTeamMode = value;
                    NotifyOfPropertyChange(() => EditTeamMode);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions editWinCondition;
        public WinConditions EditWinCondition
        {
            get { return editWinCondition; }
            set
            {
                if (value != editWinCondition)
                {
                    editWinCondition = value;
                    NotifyOfPropertyChange(() => EditWinCondition);
                }
            }
        }

        private string editAcronym;
        public string EditAcronym
        {
            get { return editAcronym; }
            set
            {
                if (value != editAcronym)
                {
                    editAcronym = value;
                    NotifyOfPropertyChange(() => EditAcronym);
                    NotifyOfPropertyChange(() => EditTournamentSaveEnabled);
                }
            }
        }

        private int editTeamSize;
        public int EditTeamSize
        {
            get { return editTeamSize; }
            set
            {
                if (value != editTeamSize)
                {
                    editTeamSize = value;
                    NotifyOfPropertyChange(() => EditTeamSize);
                    NotifyOfPropertyChange(() => EditTournamentSaveEnabled);
                }
            }
        }

        private int editRoomSize;
        public int EditRoomSize
        {
            get { return editRoomSize; }
            set
            {
                if (value != editRoomSize)
                {
                    editRoomSize = value;
                    NotifyOfPropertyChange(() => EditRoomSize);
                    NotifyOfPropertyChange(() => EditTournamentSaveEnabled);
                }
            }
        }

        private int editPointsForSecondBan;
        public int EditPointsForSecondBan
        {
            get { return editPointsForSecondBan; }
            set
            {
                if (value != editPointsForSecondBan)
                {
                    editPointsForSecondBan = value;
                    NotifyOfPropertyChange(() => EditPointsForSecondBan);
                }
            }
        }

        private bool editAllPicksFreemod;
        public bool EditAllPicksFreemod
        {
            get { return editAllPicksFreemod; }
            set
            {
                if (value != editAllPicksFreemod)
                {
                    editAllPicksFreemod = value;
                    NotifyOfPropertyChange(() => EditAllPicksFreemod);
                }
            }
        }

        private int editMpTimerCommand;
        public int EditMpTimerCommand
        {
            get { return editMpTimerCommand; }
            set
            {
                if (value != editMpTimerCommand)
                {
                    editMpTimerCommand = value;
                    NotifyOfPropertyChange(() => EditMpTimerCommand);
                }
            }
        }

        private int editMpTimerAfterGame;
        public int EditMpTimerAfterGame
        {
            get { return editMpTimerAfterGame; }
            set
            {
                if (value != editMpTimerAfterGame)
                {
                    editMpTimerAfterGame = value;
                    NotifyOfPropertyChange(() => EditMpTimerAfterGame);
                }
            }
        }

        private int editMpTimerAfterPick;
        public int EditMpTimerAfterPick
        {
            get { return editMpTimerAfterPick; }
            set
            {
                if (value != editMpTimerAfterPick)
                {
                    editMpTimerAfterPick = value;
                    NotifyOfPropertyChange(() => EditMpTimerAfterPick);
                }
            }
        }

        private string editWelcomeString;
        public string EditWelcomeString
        {
            get { return editWelcomeString; }
            set
            {
                if (value != editWelcomeString)
                {
                    editWelcomeString = value;
                    NotifyOfPropertyChange(() => EditWelcomeString);
                }
            }
        }

        public bool EditTournamentSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(editName))
                    return false;
                if (Database.Database.Tournaments.Any(x => x.Name == editName && x.Id != tournament.Id))
                    return false;
                if (string.IsNullOrEmpty(editAcronym))
                    return false;
                if (editTeamSize < 1)
                    return false;
                if (editRoomSize < 1)
                    return false;
                return true;
            }
        }

        public void Edit()
        {
            Log.Information("GUI tournament '{name}' edit dialog open", tournament.Name);
            EditName = tournament.Name;
            EditGameMode = tournament.GameMode;
            EditTeamMode = tournament.TeamMode;
            EditWinCondition = tournament.WinCondition;
            EditAcronym = tournament.Acronym;
            EditTeamSize = tournament.TeamSize;
            EditRoomSize = tournament.RoomSize;
            EditPointsForSecondBan = tournament.PointsForSecondBan;
            EditAllPicksFreemod = tournament.AllPicksFreemod;
            EditMpTimerCommand = tournament.MpTimerCommand;
            EditMpTimerAfterGame = tournament.MpTimerAfterGame;
            EditMpTimerAfterPick = tournament.MpTimerAfterPick;
            EditWelcomeString = tournament.WelcomeString;
        }

        public void Save()
        {
            if (EditTournamentSaveEnabled)
            {
                Log.Information("GUI edit tournament '{name}' save", tournament.Name);
                tournament.Name = EditName;
                tournament.GameMode = EditGameMode;
                tournament.TeamMode = EditTeamMode;
                tournament.WinCondition = EditWinCondition;
                tournament.Acronym = EditAcronym;
                tournament.TeamSize = EditTeamSize;
                tournament.RoomSize = EditRoomSize;
                tournament.PointsForSecondBan = EditPointsForSecondBan;
                tournament.AllPicksFreemod = EditAllPicksFreemod;
                tournament.MpTimerCommand = EditMpTimerCommand;
                tournament.MpTimerAfterGame = EditMpTimerAfterGame;
                tournament.MpTimerAfterPick = EditMpTimerAfterPick;
                tournament.WelcomeString = EditWelcomeString;
                tournament.Save();
                NotifyOfPropertyChange(() => Name);
            }
        }
        #endregion

        #region Actions
        public void Delete()
        {
            Log.Information("GUI delete team '{name}'", tournament.Name);
            tournament.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteTournament");
        }
        #endregion
    }
}
