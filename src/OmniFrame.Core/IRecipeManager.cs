using System;
using System.Collections.Generic;

namespace OmniFrame.Core
{
    /// <summary>
    /// 配方管理器接口
    /// </summary>
    public interface IRecipeManager : IDisposable
    {
        RecipeData CurrentRecipe { get; }
        int RecipeCount { get; }
        List<RecipeData> AllRecipes { get; }

        event EventHandler<RecipeData> RecipeLoaded;
        event EventHandler<RecipeData> RecipeSaved;
        event EventHandler<RecipeData> RecipeDeleted;
        event EventHandler<RecipeData> CurrentRecipeChanged;

        bool Initialize();
        RecipeData CreateRecipe(string name, string description = null);
        void AddRecipe(RecipeData recipe);
        bool SaveRecipe(RecipeData recipe);
        bool DeleteRecipe(string recipeId);
        bool LoadRecipe(string recipeId);
        bool LoadRecipeByName(string name);
        RecipeData CopyRecipe(string sourceRecipeId, string newName = null);
        List<RecipeData> GetRecipeList();
        bool ExportRecipe(string recipeId, string filePath);
        RecipeData ImportRecipe(string filePath);
    }
}
