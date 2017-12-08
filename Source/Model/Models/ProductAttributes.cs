using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductAttributes : EntityBase, IHistoryLog
    {      
        [Key]
        public int ProductAttributeId { get; set; }

        /*
         * public virtual Product Product { get; set; } //was not working due to below error so relying on Ids only
           public ProductTypeAttribute ProductTypeAttribute { get; set; } //was not working due to below error so relying on Ids only
         *
         Attaching an entity of type 'Model.Models.ProductType' failed because another entity of the same type already has the same primary key value. 
         * This can happen when using the 'Attach' method or setting the state of an entity to 'Unchanged' or 'Modified' if any entities in the graph have conflicting key values. 
         * This may be because some entities are new and have not yet received database-generated key values. In this case use the 'Add' method or the 'Added' entity state to track
         * the graph and then set the state of non-new entities to 'Unchanged' or 'Modified' as appropriate.        
         
         */

        public int ProductId { get; set; }
        public int ProductTypeAttributeId { get; set; }

        public string ProductAttributeValue { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }


}
