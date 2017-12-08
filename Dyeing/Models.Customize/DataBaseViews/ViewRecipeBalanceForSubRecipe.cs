using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.DataBaseViews
{
    [Table("ViewRecipeBalanceForSubRecipe")]
    public class ViewRecipeBalanceForSubRecipe : EntityBase
    {
        [Key]
        public int JobOrderHeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public int ProductId { get; set; }
        public int Dimension1Id { get; set; }
        public int? Dimension2Id { get; set; }
        public string RecipeNo { get; set; }
        public Decimal BalanceQty { get; set; }
    }
}
