using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OAuth2DemoDbContext
{
    public class OAuth_Nonce
    {
        [Key, Column(Order = 0)]
        [MaxLength(50)]
        public string Context { get; set; }
        [Key, Column(Order = 1)]
        public string Code { get; set; }
        [Key, Column(Order = 2)]
        public DateTime Timestamp { get; set; }
    }
}