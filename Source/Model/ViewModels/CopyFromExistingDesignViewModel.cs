using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class CopyFromExistingDesignViewModel
    {
        [Required,Range(1,int.MaxValue,ErrorMessage="The Design field is required")]
        public int ProductGroupId{ get; set; }
        [Required(ErrorMessage="Design Name field is required")]
        public string ProductGroupName { get; set; }
    }

    public class CopyFromExistingSaleInvoiceViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "The Sale Invoice field is required")]
        public int SaleInvoiceHeaderId { get; set; }
        [Required(ErrorMessage = "Sale Invoice field is required"),MaxLength(20)]
        public string SaleInvoiceDocNo { get; set; }
    }

    public class CopyFromExistingDesignConsumptionViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Design field is required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Design field is required")]
        public int ProductGroupId { get; set; }
    }

    public class CopyFromExistingProductConsumptionViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Product field is required")]
        public int FromProductId { get; set; }
        [Required(ErrorMessage = "Product field is required")]
        public int ToProductId { get; set; }
    }

    public class CopyFromExistingDesignColourConsumptionViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Design field is required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Design field is required")]
        public int ProductGroupId { get; set; }

        [Required(ErrorMessage = "Design field is required")]
        public int ColourId { get; set; }
    }

    public class CopyFromExistingProductViewModel
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Product field is required")]
        public int FromProductId { get; set; }

        public int? ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int? BomProductId { get; set; }
        public string BomProductName { get; set; }
        public int? ReplacingBomProductId { get; set; }
        public string ReplacingBomProductName { get; set; }



        public int? BomDimension1Id { get; set; }
        public string BomDimension1Name { get; set; }
        public int? ReplacingBomDimension1Id { get; set; }
        public string ReplacingBomDimension1Name { get; set; }



        public int? BomDimension2Id { get; set; }
        public string BomDimension2Name { get; set; }
        public int? ReplacingBomDimension2Id { get; set; }
        public string ReplacingBomDimension2Name { get; set; }



        public int? BomDimension3Id { get; set; }
        public string BomDimension3Name { get; set; }
        public int? ReplacingBomDimension3Id { get; set; }
        public string ReplacingBomDimension3Name { get; set; }




        public int? BomDimension4Id { get; set; }
        public string BomDimension4Name { get; set; }
        public int? ReplacingBomDimension4Id { get; set; }
        public string ReplacingBomDimension4Name { get; set; }

        public ProductTypeSettingsViewModel ProductTypeSettings { get; set; }


    }
}
