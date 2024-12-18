﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ShoeWeb.Identity;

namespace ShoeWeb.Models
{
    public class Order
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required(ErrorMessage = "Tên khách hàng không để trống")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số điện thoại không để trống")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa chỉ khổng để trống")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Tỉnh thành không được để trống")]
        public string TinhThanh { get; set; }
        [Required(ErrorMessage = "Quận huyện không được để trống")]
        public string QuanHuyen { get; set; } 
        [Required(ErrorMessage = "Phường/xã không được để trống")]
        public string PhuongXa { get; set; }

        public int StatusShipping { get; set; } = 0; //0 : chờ vận chuyển, 1: bàn giao cho đơn vị vận chuyển, 2: đang vận chuyển, 3: đã giao hàng, khác : giao hàng thất bại
        public bool isPayment { get; set; } = false;

        public bool isAccept { get; set; } = false;
        public string Email { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public int TypePayment { get; set; }
        public int Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public string userId { get; set; }
        [ForeignKey("userId")]
        public AppUser User { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}