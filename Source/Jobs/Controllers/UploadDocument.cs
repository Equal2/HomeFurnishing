using Data.Models;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Model.Models;
using System.IO;
using System.Collections.Generic;
using ImageResizer;
using System;
using Core.Common;

namespace Jobs.Controllers
{
    public class UploadDocument
    {
        static string uploadfolder;

        public UploadDocument()
        {
            if (string.IsNullOrEmpty(uploadfolder))
                uploadfolder = GetCurrentFolderForUpload();
        }

        public void UploadFile(int FileKey, Dictionary<string, string> versions, string FilePrefix, FileTypeConstants FType, out string FolderName, out string FileName)
        {

            //Saving Images if any uploaded after UnitOfWorkSave

            FileName = "";
            FolderName = uploadfolder;

            var Request = System.Web.HttpContext.Current.Request;

            //Define the versions to generate
            versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
            versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg

            if (FType == FileTypeConstants.Image)
            {
                string filename = System.Guid.NewGuid().ToString();


                HttpPostedFile pfile = Request.Files[FileKey];
                if (pfile.ContentLength <= 0) return; //Skip unused file controls.  

                string uploadFolder = Request.MapPath("~/Uploads/" + uploadfolder);
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                string filecontent = Path.Combine(uploadFolder, FilePrefix + "_" + filename);

                //pfile.SaveAs(filecontent);
                ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));

                FileName = filename;

                //Generate each version
                foreach (string suffix in versions.Keys)
                {
                    if (suffix == "_thumb")
                    {
                        string tuploadFolder = Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                        if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                        //Generate a filename (GUIDs are best).
                        string tfileName = Path.Combine(tuploadFolder, FilePrefix + "_" + filename);

                        //Let the image builder add the correct extension based on the output file type                        
                        ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                    }
                    else if (suffix == "_medium")
                    {
                        string tuploadFolder = Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                        if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                        //Generate a filename (GUIDs are best).
                        string tfileName = Path.Combine(tuploadFolder, FilePrefix + "_" + filename);

                        //Let the image builder add the correct extension based on the output file type                       
                        ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                    }

                }

            }
            else if (FType == FileTypeConstants.Other)
            {


                HttpPostedFile pfile = Request.Files[FileKey];
                if (pfile.ContentLength <= 0) return; //Skip unused file controls.  

                string uploadFolder = Request.MapPath("~/Uploads/" + uploadfolder);

                FileName = FilePrefix + "_" + pfile.FileName;

                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                string filecontent = Path.Combine(uploadFolder, FileName);

                pfile.SaveAs(filecontent);


            }










        }


        public string GetCurrentFolderForUpload()
        {
            //For checking the first time if the folder exists or not-----------------------------
            string uploadfolder;
            int MaxLimit;
            string UserName = System.Web.HttpContext.Current.User.Identity.Name;
            var Request = System.Web.HttpContext.Current.Request;

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                int.TryParse(ConfigurationManager.AppSettings["MaxFileUploadLimit"], out MaxLimit);
                var x = (from iid in db.Counter
                         select iid).FirstOrDefault();
                if (x == null)
                {

                    uploadfolder = System.Guid.NewGuid().ToString();
                    Counter img = new Counter();
                    img.ImageFolderName = uploadfolder;
                    img.ModifiedBy = UserName;
                    img.CreatedBy = UserName;
                    img.ModifiedDate = DateTime.Now;
                    img.CreatedDate = DateTime.Now;
                    img.ObjectState = Model.ObjectState.Added;
                    db.Counter.Add(img);

                    db.SaveChanges();
                }

                else
                { uploadfolder = x.ImageFolderName; }


                //For checking if the image contents length is greater than 100 then create a new folder------------------------------------

                if (!Directory.Exists(Request.MapPath("~/Uploads/" + uploadfolder))) Directory.CreateDirectory(Request.MapPath("~/Uploads/" + uploadfolder));

                int count = Directory.GetFiles(Request.MapPath("~/Uploads/" + uploadfolder)).Length;

                if (count >= MaxLimit)
                {
                    uploadfolder = System.Guid.NewGuid().ToString();

                    var u = db.Counter.Find(x.CounterId);
                    u.ImageFolderName = uploadfolder;
                    u.ObjectState = Model.ObjectState.Modified;
                    db.Counter.Add(u);
                    db.SaveChanges();
                }

            }

            return uploadfolder;
        }

    }
}
