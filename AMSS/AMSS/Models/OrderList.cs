using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMSS.Models
{
    public class OrderList
    {
        [Key]
        public int OrderListId { get; set; }

        public string UserId { get; set; }
        public int RestaurantId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Restaurant Restaurant { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}