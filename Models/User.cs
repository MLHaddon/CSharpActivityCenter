using System.Text.RegularExpressions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ExamProject.Models
{
    public class User
    {
        [Key]
        public int UserId {get; set;}

        [Required]
        [MinLength(2, ErrorMessage = "First Name must be atleast Two characters in length.")]
        [Display(Name ="First Name")]
        public string FirstName {get; set;}

        [Required]
        [MinLength(2, ErrorMessage = "Last Name must be atleast Two characters in length.")]
        [Display(Name ="Last Name")]
        public string LastName {get; set;}

        [Required]
        [EmailAddress]
        public string Email {get; set;}

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[@#$%^&-+=()])(?=\\S+$).{8,20}$", ErrorMessage = "Password Must contain at least ONE Letter, Number, and Special Character.")]
        [DataType(DataType.Password)]
        public string Password {get; set;}


        [MinLength(8, ErrorMessage = "Password Confirmation must match the password.")]
        [Compare("Password", ErrorMessage = "Password must match.")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[@#$%^&-+=()])(?=\\S+$).{8,20}$", ErrorMessage = "Password Must contain at least ONE Letter, Number, and Special Character.")]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm Password")]
        [NotMapped]
        public string ConfirmPassword {get; set;}

        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }

        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;

        public List<DojoActivity> CreatedActivities {get; set;}
        public List<Association> JoinedActivities {get; set;}
    }
}