using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class ProductCustomGroupExcelViewModel
    {
        public int ProductCustomGroupHeaderId { get; set; }
        public string ProductCustomGroupName { get; set; }
        public string ProductName { get; set; }
        public Decimal Qty { get; set; }
    }
}
