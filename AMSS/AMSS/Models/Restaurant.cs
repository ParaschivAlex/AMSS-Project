using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMSS.Models
{
    public class Restaurant
    {
        //name, location, reviews
        [Key]
        public int RestaurantId { get; set; }

        [Required(ErrorMessage = "Numele restaurantului este obligatoriu")]
        public string RestaurantName { get; set; }

        public string RestaurantLocation { get; set; }

        [Required(ErrorMessage = "Poza e obligatorie")]
        public string RestaurantPhoto { get; set; }

        public int RestaurantRating { get; set; }

        public virtual ICollection<Food> Foods { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}