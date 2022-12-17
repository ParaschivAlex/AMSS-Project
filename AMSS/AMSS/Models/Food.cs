using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AMSS.Models
{
    public class Food
    {
        [Key]
        public int FoodId { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [StringLength(60, ErrorMessage = "Numele nu poate contine mai mult de 60 caractere")]
        public string FoodName { get; set; }

        [DataType(DataType.MultilineText)]
        public string FoodIngredients { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Pretul nu poate fi negativ")]
        public float FoodPrice { get; set; }

        [Required(ErrorMessage = "Poza e obligatorie")]
        public string FoodPhoto { get; set; }

        [Required(ErrorMessage = "Restaurantul este obligatoriu")]
        public int RestaurantId { get; set; }

        public DateTime FoodModifyDate { get; set; }

        public IEnumerable<SelectListItem> RestaurantList { get; set; }

        public virtual Restaurant Restaurant { get; set; }
        
    }
}