using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class DocumentCancelViewModel : EntityBase
    {
        public DocumentCancelViewModel()
        {
        }
        public int HeaderId { get; set; }
        public DateTime DocDate { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string Remark { get; set; }
    }
}
