using Data.Models;
using ImageResizer;
using Model.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Presentation;
using System.Net;

namespace Jobs.Areas.Rug.Controllers
{
    public class ImageULWizardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /ImageULWizard/
        public ActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload(FormCollection Collection)
        {

            var file = Request.Files.Keys;
            int i = 0;
            string FailedDesigns = "";
            string Error = "";
            foreach (var Img in file)
            {


                if (Request.Files[i] != null && Request.Files[i].ContentLength > 0)
                {


                    string DesignName = Request.Files[i].FileName;

                    DesignName = DesignName.Replace(Path.GetExtension(Request.Files[i].FileName), "");

                    var Group = (from p in db.ProductGroups
                                 where p.ProductGroupName.ToLower() == DesignName.ToLower()
                                 select p).FirstOrDefault();




                    if (Group != null)
                    {
                        //Deleting Existing Images
                        if (Group.ImageFolderName != null && Group.ImageFileName != null)
                        {    //Deleting Existing Images

                            var xtemp = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/" + Group.ImageFileName);
                            if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/" + Group.ImageFileName)))
                            {
                                System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/" + Group.ImageFileName));
                            }

                            //Deleting Thumbnail Image:

                            if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/Thumbs/" + Group.ImageFileName)))
                            {
                                System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/Thumbs/" + Group.ImageFileName));
                            }

                            //Deleting Medium Image:
                            if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/Medium/" + Group.ImageFileName)))
                            {
                                System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + Group.ImageFolderName + "/Medium/" + Group.ImageFileName));
                            }
                        }






                        //For checking the first time if the folder exists or not-----------------------------
                        string uploadfolder;
                        int MaxLimit;
                        int.TryParse(ConfigurationManager.AppSettings["MaxFileUploadLimit"], out MaxLimit);
                        var x = (from iid in db.Counter
                                 select iid).FirstOrDefault();
                        if (x == null)
                        {

                            uploadfolder = System.Guid.NewGuid().ToString();
                            Counter img = new Counter();
                            img.ImageFolderName = uploadfolder;
                            img.ModifiedBy = User.Identity.Name;
                            img.CreatedBy = User.Identity.Name;
                            img.ModifiedDate = DateTime.Now;
                            img.CreatedDate = DateTime.Now;
                            img.ObjectState = Model.ObjectState.Added;
                            db.Counter.Add(img);
                        }

                        else
                        { uploadfolder = x.ImageFolderName; }


                        //For checking if the image contents length is greater than 100 then create a new folder------------------------------------

                        if (!Directory.Exists(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder))) Directory.CreateDirectory(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder));

                        int count = Directory.GetFiles(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder)).Length;

                        if (count >= MaxLimit)
                        {
                            uploadfolder = System.Guid.NewGuid().ToString();
                            var u = db.Counter.Find(x.CounterId);
                            u.ImageFolderName = uploadfolder;
                            u.ObjectState = Model.ObjectState.Modified;
                            db.Counter.Add(u);
                        }


                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 200x200 area, jpeg

                        string Extension = "";
                        string filename = System.Guid.NewGuid().ToString();
                        string UniqFileName = Group.ProductGroupName + "_" + filename;

                        HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[i];

                        if (pfile.ContentLength <= 0) continue; //Skip unused file controls.  

                        Extension = Path.GetExtension(pfile.FileName);

                        string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                        string filecontent = Path.Combine(uploadFolder, UniqFileName);

                        //pfile.SaveAs(filecontent);
                        ImageBuilder.Current.Build(new ImageJob(pfile, filecontent, new Instructions(), false, true));


                        //Generate each version
                        foreach (string suffix in versions.Keys)
                        {
                            if (suffix == "_thumb")
                            {
                                string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Thumbs");
                                if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                //Generate a filename (GUIDs are best).
                                string tfileName = Path.Combine(tuploadFolder, UniqFileName);

                                //Let the image builder add the correct extension based on the output file type
                                ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                            }
                            else if (suffix == "_medium")
                            {
                                string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                //Generate a filename (GUIDs are best).
                                string tfileName = Path.Combine(tuploadFolder, UniqFileName);

                                //Let the image builder add the correct extension based on the output file type
                                ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                            }

                        }


                        Group.ImageFileName = UniqFileName + Extension;
                        Group.ImageFolderName = uploadfolder;
                        Group.ObjectState = Model.ObjectState.Modified;

                        db.ProductGroups.Add(Group);


                        var Products = (from p in db.Product
                                        where p.ProductGroupId == Group.ProductGroupId
                                        select p).ToList();

                        foreach (var Prod in Products)
                        {

                            Prod.ImageFileName = Group.ImageFileName;
                            Prod.ImageFolderName = Group.ImageFolderName;

                            Prod.ObjectState = Model.ObjectState.Modified;
                            db.Product.Add(Prod);

                        }



                    }
                    else
                        FailedDesigns += DesignName;
                }
                i++;
            }

            db.SaveChanges();

            if (string.IsNullOrEmpty(FailedDesigns))
                return Json(true);
            else
                return Json("Failed to find design for " + FailedDesigns);
        }



        //[HttpPost]
        //public ActionResult Upload(FormCollection Collection)
        //{
        //    bool isSavedSuccessfully = true;
        //    string fName = "";
        //    foreach (string fileName in Request.Files)
        //    {
        //        HttpPostedFileBase file = Request.Files[fileName];
        //        //Save file content goes here
        //        fName = file.FileName;
        //        if (file != null && file.ContentLength > 0)
        //        {

        //            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\WallImages", Server.MapPath(@"\")));

        //            string pathString = System.IO.Path.Combine(originalDirectory.ToString(), "imagepath");

        //            var fileName1 = Path.GetFileName(file.FileName);


        //            bool isExists = System.IO.Directory.Exists(pathString);

        //            if (!isExists)
        //                System.IO.Directory.CreateDirectory(pathString);

        //            var path = string.Format("{0}\\{1}", pathString, file.FileName);
        //            file.SaveAs(path);

        //        }

        //    }

        //    if (isSavedSuccessfully)
        //    {
                
        //    }
        //    else
        //    {
        //        return Json("Could not find Design");
        //    }
        //}


    }
}