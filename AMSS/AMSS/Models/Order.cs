using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMSS.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Adresa la care doriti sa livram mancarea este obligatorie")]
        public string Address { get; set; }

        public float Payment { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }

        public int RestaurantId { get; set; }

        public string UserId { get; set; }

        public string DeliveryUserId { get; set; }


        public virtual ApplicationUser User { get; set; }
        public virtual Restaurant Restaurant { get; set; }
        public virtual ApplicationUser DeliveryUser { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }


    }
}