using System.ComponentModel.DataAnnotations;



namespace slbfeHardware.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Employee number is required.")]
        public string EmpNo { get; set; }

        [Required(ErrorMessage = "Existing password is required.")]
        [DataType(DataType.Password)]
        public string ExistingPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The new password must be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}