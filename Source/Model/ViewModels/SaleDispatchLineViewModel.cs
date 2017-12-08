using Model.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SaleDispatchLineFilterViewModel
    {
        public int SaleDispatchHeaderId { get; set; }
        public int SupplierId { get; set; }
        [Display(Name = "Sale Order No")]
        public string SaleOrderHeaderId { get; set; }
        [Display(Name = "Product")]
        public string ProductId { get; set; }
        [Display(Name = "Product Group")]
        public string ProductGroupId { get; set; }
    }
    public class SaleDispatchMasterDetailModel
    {
        public List<SaleDispatchLineViewModel> SaleDispatchLineViewModel { get; set; }
        public SaleDispatchSettingsViewModel SaleDispatchSettings { get; set; }
    }

}
