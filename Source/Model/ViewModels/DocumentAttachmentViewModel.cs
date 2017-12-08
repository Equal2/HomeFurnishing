
using Core.Common;
namespace Model.ViewModel
{
    public class DocumentAttachmentViewModel
    {
        public int DocumentAttachmentId { get; set; }
        public int DocTypeId { get; set; }
        public int DocId { get; set; }
        public string FileFolderName { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string IcoClass { get; set; }

        public void SetExtension()
        {
            if(this.FileName.Contains(ExtensionConstants.BMP) || this.FileName.Contains(ExtensionConstants.JPEG) || this.FileName.Contains(ExtensionConstants.JPG)|| this.FileName.Contains(ExtensionConstants.PNG))
            {
                this.Extension = ExtensionConstants.JPG;
                this.IcoClass = "fa fa-file-image-o fa-2x image atch";
            }
            else if(this.FileName.Contains(ExtensionConstants.XLS) || this.FileName.Contains(ExtensionConstants.XLSX))
            {
                this.Extension=ExtensionConstants.XLS;
                this.IcoClass = "fa fa-file-excel-o fa-2x excel atch";
            }
            else if(this.FileName.Contains(ExtensionConstants.DOC))
            {
                this.Extension = ExtensionConstants.DOC;
                this.IcoClass = "fa fa-file-word-o fa-2x word atch";
            }
            else if(this.FileName.Contains(ExtensionConstants.PDF))
            {
                this.Extension = ExtensionConstants.PDF;
                this.IcoClass = "fa fa-file-pdf-o fa-2x pdf atch";
            }
            else
            {
                this.Extension = ExtensionConstants.NA;
                this.IcoClass = "fa fa-file-o fa-2x";
            }
        }

    }    
}
