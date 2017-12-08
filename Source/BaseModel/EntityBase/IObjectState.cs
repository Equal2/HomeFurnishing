
using System.ComponentModel.DataAnnotations.Schema;

namespace Surya.India.Model
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}