using System.IO;
using System.Web;

namespace CCSortApp.Utility {
    public class TempFile {
        public void create(IFormFile file, string uploads) {
            if (file.Length > 0) {
                string filePath = Path.Combine(uploads, file.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                    file.CopyTo(fileStream);
                }
            }
        }

        public void trash(string filepath) {
            if(File.Exists(filepath))
               File.Delete(filepath); 
        }
    }

}