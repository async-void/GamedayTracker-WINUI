using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Factories;
using GamedayTracker.Services.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;

namespace GamedayTracker.ViewModels.Pages
{
    public partial class AddPlayerPageViewModel : ObservableObject
    {
        private readonly PlayerDbContextFactory _dbFactory;
        public ISnackbarMessageQueue MsgQueue { get; }
        public AddPlayerPageViewModel(PlayerDbContextFactory factory, ISnackbarMessageQueue msgQue)
        {
            _dbFactory = factory;
            MsgQueue = msgQue;
            LoadPlayers();
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string? _name;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string? _company;

        [ObservableProperty] 
        private ObservableCollection<Player> _players = [];

        [ObservableProperty] private bool _isValid;

        #region SUBMIT COMMAND
        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task OnSubmit()
        {
            await using var db = _dbFactory.CreateDbContext();
            var player = db.Players.FirstOrDefault(x => x.Name.Equals(Name));

            if (player is null)
            {
                var p = new Player()
                {
                    Name = Name!,
                    Company = Company!   
                };
                db.Players.Add(p);
                await db.SaveChangesAsync();
                IsValid = true;
                MsgQueue.Enqueue($"Player {Name} Saved!");
            }
            else
            {
                IsValid = false;
                MsgQueue.Enqueue($"Player {Name} already exists!");
            }
               
            

            Name = string.Empty;
            Company = string.Empty;
        }
        #endregion

        #region PLAYER PICKS COMMAND
        [RelayCommand]
        private void OnPlayerPicks(Player player)
        {
            var test = "";
        }
        #endregion

        #region EDIT PLAYER COMMAND
        [RelayCommand]
        private void OnEditPlayer(Player player)
        {
            Name = player.Name;
            Company = player.Company;
        }
        #endregion

        private bool CanSubmit() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Company);

        private void LoadPlayers()
        {
            var db = _dbFactory.CreateDbContext();
            Players = [];
            var players = db.Players.ToList();
            foreach (var player in players)
            {
                Players.Add(player);
            }
        }
    }

}
