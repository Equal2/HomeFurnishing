using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Model.Models;
using Model.ViewModels;
using Data.Models;
using Data.Infrastructure;
using Service;
using AutoMapper;
using Presentation.ViewModels;
using Presentation;
using Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Text;
using System.Configuration;
using System.IO;
using ImageResizer;
using Model.ViewModel;
using System.Drawing;


namespace Jobs.Areas.Rug.Controllers
{
   [Authorize]
    public class ProductImageController : System.Web.Mvc.Controller
    {
       private ApplicationDbContext db = new ApplicationDbContext();
       IUnitOfWork _unitOfWork;
       IProductGroupService _ProductGroupService;
       string path = "E:\\Designs\\";

       public ProductImageController(IUnitOfWork unitOfWork, IProductGroupService group)
        {
            _ProductGroupService = group;
            _unitOfWork = unitOfWork;
        }
        // GET: /Order/

        public ActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post()
        {
            string DesignImageName = "";

            var productgroups = (from Pg in db.ProductGroups 
                                 where Pg.ImageFileName == null
                                 orderby Pg.ProductGroupName descending
                     select new
                     {
                         ProductDesignName = Pg.ProductGroupName
                     }).ToList();


            foreach (var item in productgroups)
            {
                ProductGroup ProductGroup = new ProductGroupService(_unitOfWork).Find(item.ProductDesignName);

                DesignImageName = item.ProductDesignName.ToString().Replace("-", "").Replace("/","") + "-58";


                try
                {


                    var file = Directory.GetFiles(path, DesignImageName + ".png", SearchOption.AllDirectories);



                    #region

                    //Saving Images if any uploaded after UnitOfWorkSave

                    if (file != null && file.Length > 0)
                    {
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
                            new CounterService(_unitOfWork).Create(img);
                            _unitOfWork.Save();
                        }

                        else
                        { uploadfolder = x.ImageFolderName; }


                        //For checking if the image contents length is greater than 100 then create a new folder------------------------------------

                        if (!Directory.Exists(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder))) Directory.CreateDirectory(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder));

                        int count = Directory.GetFiles(System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder)).Length;

                        if (count >= MaxLimit)
                        {
                            uploadfolder = System.Guid.NewGuid().ToString();
                            var u = new CounterService(_unitOfWork).Find(x.CounterId);
                            u.ImageFolderName = uploadfolder;
                            new CounterService(_unitOfWork).Update(u);
                            _unitOfWork.Save();
                        }


                        //Saving Thumbnails images:
                        Dictionary<string, string> versions = new Dictionary<string, string>();

                        //Define the versions to generate
                        versions.Add("_thumb", "maxwidth=100&maxheight=100"); //Crop to square thumbnail
                        versions.Add("_medium", "maxwidth=200&maxheight=200"); //Fit inside 400x400 area, jpeg

                        string temp2 = "";
                        string filename = System.Guid.NewGuid().ToString();



                        // Load file meta data with FileInfo
                        FileInfo fileInfo = new FileInfo(path + DesignImageName + ".png");

                        // The byte[] to save the data in
                        byte[] data = new byte[fileInfo.Length];

                        // Load a filestream and put its content into the byte[]
                        using (FileStream fs = fileInfo.OpenRead())
                        {
                            fs.Read(data, 0, data.Length);
                        }

                        // Delete the temporary file
                        fileInfo.Delete();


                        //System.IO.FileStream _FileStream = new System.IO.FileStream(DesignImageName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                        //_FileStream.Write(data, 0, data.Length);
                        //_FileStream.Close();

                        Image pfile = (Bitmap)((new ImageConverter()).ConvertFrom(data));

                        //HttpPostedFile pfile = System.Web.HttpContext.Current.Request.Files[DesignImageName];


                        //if (pfile.ContentLength <= 0) continue; //Skip unused file controls.  

                        //temp2 = Path.GetExtension(pfile.FileName);
                        temp2 = ".PNG";

                        string uploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder);
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                        string filecontent = Path.Combine(uploadFolder, ProductGroup.ProductGroupName + "_" + filename);

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
                                string tfileName = Path.Combine(tuploadFolder, item.ProductDesignName + "_" + filename);

                                //Let the image builder add the correct extension based on the output file type
                                //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));

                            }
                            else if (suffix == "_medium")
                            {
                                string tuploadFolder = System.Web.HttpContext.Current.Request.MapPath("~/Uploads/" + uploadfolder + "/Medium");
                                if (!Directory.Exists(tuploadFolder)) Directory.CreateDirectory(tuploadFolder);

                                //Generate a filename (GUIDs are best).
                                string tfileName = Path.Combine(tuploadFolder, item.ProductDesignName + "_" + filename);

                                //Let the image builder add the correct extension based on the output file type
                                //fileName = ImageBuilder.Current.Build(file, fileName, new ResizeSettings(versions[suffix]), false, true);
                                ImageBuilder.Current.Build(new ImageJob(pfile, tfileName, new Instructions(versions[suffix]), false, true));
                            }

                        }

                        //var tempsave = _FinishedProductService.Find(pt.ProductId);

                        ProductGroup.ImageFileName = item.ProductDesignName + "_" + filename + temp2;
                        ProductGroup.ImageFolderName = uploadfolder;
                        ProductGroup.ObjectState = Model.ObjectState.Modified;



                        //IEnumerable<Product> products = (from p in db.Product
                        //                                 where p.ProductGroupId == ProductGroup.ProductGroupId
                        //                                 select p).ToList();

                        //foreach (Product p in products)
                        //{
                        //    p.ImageFileName = item.ProductDesignName + "_" + filename + temp2;
                        //    p.ImageFolderName = uploadfolder;
                        //    new ProductService(_unitOfWork).Update(p);
                        //}





                        _ProductGroupService.Update(ProductGroup);

                        try
                        {
                            _unitOfWork.Save();
                        }
                        catch (DbEntityValidationException dbe)
                        {
                            string Msg = dbe.Message;
                        }
                        catch (Exception e)
                        {
                            string Msg = e.Message;
                        }



                    }

                    #endregion

                }
                catch (Exception e)
                {
                    string Msg = e.Message;
                }
            }


            return View("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
