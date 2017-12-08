using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    [Serializable]
    public class DocumentTypeSettingsViewModel
    {
        [Key]
        public int DocumentTypeSettingsId { get; set; }
        public int DocumentTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }

        [MaxLength(50)]
        public string PartyCaption { get; set; }

        [MaxLength(50)]
        public string ProductUidCaption { get; set; }

        [MaxLength(50)]
        public string ProductCaption { get; set; }

        [MaxLength(50)]
        public string ProductGroupCaption { get; set; }

        [MaxLength(50)]
        public string ProductCategoryCaption { get; set; }

        [MaxLength(50)]
        public string Dimension1Caption { get; set; }

        [MaxLength(50)]
        public string Dimension2Caption { get; set; }

        [MaxLength(50)]
        public string Dimension3Caption { get; set; }

        [MaxLength(50)]
        public string Dimension4Caption { get; set; }

        [MaxLength(50)]
        public string ContraDocTypeCaption { get; set; }

        [MaxLength(50)]
        public string DealQtyCaption { get; set; }

        [MaxLength(50)]
        public string WeightCaption { get; set; }


        [MaxLength(50)]
        public string CostCenterCaption { get; set; }

        [MaxLength(50)]
        public string SpecificationCaption { get; set; }
        [MaxLength(50)]
        public string ReferenceDocTypeCaption { get; set; }

        [MaxLength(50)]
        public string ReferenceDocIdCaption { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
