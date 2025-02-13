﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Vtest94.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ArbitraryUsername { get; set; }

        // Navigation property for related Videos
        public virtual List<Video> Videos { get; set; } = new List<Video>();

        // Navigation property for UserPhoto
        public virtual UserPhoto UserPhoto { get; set; }
    }
}
