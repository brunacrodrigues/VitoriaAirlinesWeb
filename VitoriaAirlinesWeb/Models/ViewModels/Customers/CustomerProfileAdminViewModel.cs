﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VitoriaAirlinesWeb.Models.ViewModels.Customers
{
    public class CustomerProfileAdminViewModel
    {
        public int Id { get; set; }


        [Display(Name = "Nationality")]
        public int? CountryId { get; set; }


        public IEnumerable<SelectListItem>? Countries { get; set; }


        [Display(Name = "Passport Number")]
        [MaxLength(20)]
        public string? PassportNumber { get; set; }


        public string FullName { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;

    }
}
