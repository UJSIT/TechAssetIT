// Models/LoginModel.cs
using System.ComponentModel.DataAnnotations;


namespace slbfeHardware.Models
{
    public class LoginModel
    {
        //[Required(ErrorMessage = "Username is required.")]
        public string EmpNo { get; set; }

        //[Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public ChangePasswordModel ChangePassword { get; set; } // Optional, if you want to nest
    }
}


