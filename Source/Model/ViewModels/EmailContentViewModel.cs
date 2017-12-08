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
    public class EmailContentViewModel
    {
        public string EmailUser { get; set; }
        public string EmailUserName { get; set; }
        public string EmailToStr { get; set; }
        public string EmailCcStr { get; set; }
        public string EmailBCcStr { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string FileNameList { get; set; }
    }
}
