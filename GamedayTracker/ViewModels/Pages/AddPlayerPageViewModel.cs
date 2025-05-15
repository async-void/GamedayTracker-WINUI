using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GamedayTracker.Services.Factories;
using GamedayTracker.Services.Models;

namespace GamedayTracker.ViewModels.Pages
{
    public partial class AddPlayerPageViewModel(PlayerDbContextFactory factory) : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string? _name;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string? _company;

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task OnSubmit()
        {
            await using var db = factory.CreateDbContext();
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
            }

            Name = string.Empty;
            Company = string.Empty;
        }

       private bool CanSubmit() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Company);
    }

}
