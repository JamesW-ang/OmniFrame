using System.Collections.ObjectModel;
using System.Windows.Input;
using OmniFrame.Core;

namespace OmniFrame.Wpf.ViewModels
{
    public class RecipeViewModel : ViewModelBase
    {
        private readonly IRecipeManager _recipeMgr;
        public ObservableCollection<RecipeItem> Recipes { get; } = new();
        private RecipeItem _selectedRecipe;
        public RecipeItem SelectedRecipe { get => _selectedRecipe; set => Set(ref _selectedRecipe, value); }
        private string _recipeDetail;
        public string RecipeDetail { get => _recipeDetail; set => Set(ref _recipeDetail, value); }
        public ICommand RefreshCommand { get; }

        public RecipeViewModel(IRecipeManager recipeMgr)
        {
            _recipeMgr = recipeMgr;
            RefreshCommand = new RelayCommand(LoadRecipes);
            LoadRecipes();
        }

        private void LoadRecipes()
        {
            Recipes.Clear();
            foreach (var r in _recipeMgr.GetRecipeList())
                Recipes.Add(new RecipeItem { Id = r.Id, Name = r.Name, Version = r.Version });
        }
    }

    public class RecipeItem { public string Id { get; set; } public string Name { get; set; } public string Version { get; set; } }
}
