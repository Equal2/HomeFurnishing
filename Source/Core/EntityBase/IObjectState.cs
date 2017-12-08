
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}