using System.ComponentModel.DataAnnotations;

namespace netcore_crud.Models.CardFile
{
    public class CardFileTypeSetupModel
    {
     
        public string? CardFileType { get; set; }
        public string? CardFileTypeAbbr { get; set; }
        public decimal RowStamp { get; set; }
    }
}
