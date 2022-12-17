using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMSS.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [StringLength(2048, ErrorMessage = "Comentariu maxim de 2048 caractere.")]
        public string ReviewComment { get; set; }

        [Required(ErrorMessage = "Rating obligatoriu")]
        [Range(1, 5, ErrorMessage = "Ratingul trebuie sa fie intre 1 si 5")]
        public int ReviewGrade { get; set; }

        public DateTime ReviewModifyDate { get; set; }

        public int RestaurantId { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Restaurant Restaurant { get; set; }
    }
}