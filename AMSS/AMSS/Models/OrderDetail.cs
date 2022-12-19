using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AMSS.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        public float Price { get; set; }

        public int Quantity { get; set; }

        public int FoodId { get; set; }

        public int OrderListId { get; set; }

        public virtual Food Food { get; set; }
        public virtual OrderList OrderList { get; set; }
    }
}