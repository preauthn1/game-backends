using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HangfireJob.EFModels.Gash
{
    [Table("GashPaySettle", Schema = "Pay")]
    public class GashSettleLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string MsgType { get; set; }
        [Required]
        public string PCode { get; set; }
        [Required]
        public string CId { get; set; }
        [Required]
        public string COId { get; set; }
        [Required]
        public string CUId { get; set; }
        [Required]
        public string PAId { get; set; }
        [Required]
        public string Amount { get; set; }
        [Required]
        public string ERQC { get; set; }
        [Required]
        public bool IsSuccess { get; set; }
        [Required]
        public DateTime LastUpdateTime { get; set; }
    }
}
