using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OAuth2DemoDbContext
{
    public class OAuth_ClientOpenApi
    {
        [Key, Column(Order = 0)]
        public int ClientId { get; set; }
        [Key, Column(Order = 1)]
        public string OpenApi { get; set; }
    }
}
