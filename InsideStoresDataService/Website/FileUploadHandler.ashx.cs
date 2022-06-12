using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Telerik.Windows.Controls;

namespace Website
{
    // Article on lifecycle of RadUploadHandler
    // http://www.telerik.com/support/kb/silverlight/upload/uploadhandler-uniquefilename.aspx
    
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class FileUploadHandler : Telerik.Windows.RadUploadHandler 
    {
        private readonly string uploadFolder;

        public FileUploadHandler()
        {
            uploadFolder = WebConfigurationManager.AppSettings["FileUploadsFolder"];
        }


        /// <summary>
        /// Given a reference, create the filename.
        /// </summary>
        /// <param name="fileRef"></param>
        /// <returns></returns>
        private string MakeFilename(string fileRef)
        {
                return fileRef; // for now, the ref is a filename
        }
        
        /// <summary>
        /// Returns a unique reference for the uploaded file.
        /// </summary>
        private string MakeFileReference(string userFilename)
        {
            string ext = Path.GetExtension(userFilename);
            if (string.IsNullOrEmpty(ext))
                ext= ".dat";

            var fileRef = string.Format("{0}{1}", Guid.NewGuid().ToString().Replace("-", ""), ext); 

            return fileRef;
        }

        static private byte[] ReadBinaryFile(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                // Create a byte array of file stream length   
                var ImageData = new byte[fs.Length];
                //Read block of bytes from stream into the byte array   
                fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));
                return ImageData;
                //return the byte data
            }
        }   
                
        /// <summary>
        /// Filepath where the uploaded file will be stored.
        /// </summary>
        /// <remarks>
        /// This method gets invoked internally and returns the absolute path of the file. 
        /// Therefore, if we want to save the file under different name, we will have to override this method.
        /// Although there also GetFilePath(), this is not what gets called internally. The Rad
        /// process request handler calls this version here with the filename from the request forms.
        /// </remarks>
        /// <returns></returns>
        public override string GetFilePath(string fileName)
        {
            // we want to ignore the name provided by SL client, and make our own name.
            return Path.Combine(uploadFolder, MakeFilename(ResultChunkTag));
        }

        /// <summary>
        /// Return custom data back to caller.
        /// </summary>
        /// <remarks>
        /// All these name/value pairs are combined into a single entry with key=RadUAG_associatedData in the dic
        /// passed back to the caller. Queried only on final chunk.
        /// </remarks>
        /// <returns></returns>
        public override Dictionary<string, object> GetAssociatedData()
        {
            // Retrieve in caller like this:
            //
            // private void RadUpload_FileUploaded(object sender, FileUploadedEventArgs e)
            // {
            //   RadUploadSelectedFile uploadedFile = e.SelectedFile;
            //   string myParam1Value = e.HandlerData.CustomData["myParam1"].ToString();
            // }        


            var dict = new Dictionary<string, object>
                {
                    {"Filename", ResultChunkTag},
                };

            try
            {
                var fileInfo = new FileInfo(GetFilePath());
                int filesize = (int)fileInfo.Length;

                dict.Add("Filesize", filesize);

                dict.Add("IsValid", true);

            }
            catch (Exception Ex)
            {
                dict.Add("IsValid", false);
                dict.Add("Exception", Ex.Message);
                Debug.WriteLine("Exception: " + Ex.ToString());
            }

            return dict;
        }

        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);
        }

        /// <summary>
        /// Called possibly multiple times during upload - once for each chunk sent.
        /// </summary>
        /// <remarks>
        /// Default chunk size is 100K.
        /// </remarks>
        public override void ProcessStream()
        {
             string clientFilename = Request.Form["0_RadUAG_fileName"];
        
             // Check whether this is the first time ProcessStream is invoked.  
             // If this is the case, create the new file name using a guid  
             // The next time ProcessStream gets invoked, we will use the name that is set  
             // the first time we enter ProcessStream  

             if (IsNewFileRequest())  
             {  
                 ResultChunkTag = MakeFileReference(clientFilename);
             }  
             else if (FormChunkTag != null)  
             {  
                 ResultChunkTag = FormChunkTag;  
             }  
       
            base.ProcessStream();
            
            if (IsFinalFileRequest())
            {
                AddReturnParam(RadUploadConstants.ParamNameFileName, clientFilename);
                AddReturnParam(RadUploadConstants.ParamNameChunkTag, ResultChunkTag); 
            }
        }

        public override void CancelRequest()
        {
            base.CancelRequest();
        }

    }
}
