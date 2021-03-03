using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExamProject.Models
{
    public class DojoActivity
    {
        [Key]
        public int DojoActivityId {get; set;}

        [Required(ErrorMessage = "Must have a Title")]
        [MinLength(2, ErrorMessage = "At least 2 chars")]
        public string Name {get; set;}

        [Required(ErrorMessage = "Must have a duration")]
        public string Duration {get; set;}

        [Required(ErrorMessage = "Must have a date and time")]
        public DateTime DateAndTime {get; set;}

        [Required(ErrorMessage = "Must have a description")]
        public string Description {get; set;}

        public bool IsArchived {get; set;} = false;

        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public User Coordinator {get; set;}
        public List<Association> Participants {get; set;}
    }
}