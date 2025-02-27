using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using slbfeHardware.Models;


namespace slbfeHardware.Controllers
{

    public class AccountController : Controller
    {
        public ActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UserLogin(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                    {
                        string query = @"
                SELECT uc.[Id], uc.[EmpNo], uc.[PasswordHash], ur.[ProgramId], ed.[Name_with_ini], ur.[RoleStatus] 
                FROM [TechAssets].[dbo].[Hardware_UserCredential] uc 
                LEFT JOIN [TechAssets].[dbo].[Hardware_UserRole] ur ON uc.[EmpNo] = ur.[EmpNo] 
                LEFT JOIN [EMMSDB].[dbo].[Staff_employee_Details] ed ON uc.[EmpNo] = ed.[Emp_no] 
                WHERE uc.[EmpNo] = @EmpNo";

                        SqlCommand command = new SqlCommand(query, conn_uat);
                        command.Parameters.AddWithValue("@EmpNo", model.EmpNo);

                        conn_uat.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        List<string> programIds = new List<string>();
                        Dictionary<string, int> programRoleStatus = new Dictionary<string, int>();
                        string storedPasswordHash = null;
                        string employeeName = null;

                        while (reader.Read())
                        {
                            if (storedPasswordHash == null)
                            {
                                storedPasswordHash = reader["PasswordHash"].ToString();
                                employeeName = reader["Name_with_ini"].ToString();
                            }

                            var programId = reader["ProgramId"]?.ToString();
                            var roleStatus = reader["RoleStatus"] != DBNull.Value ? Convert.ToInt32(reader["RoleStatus"]) : 0;

                            if (!string.IsNullOrEmpty(programId))
                            {
                                programIds.Add(programId);
                                programRoleStatus[programId] = roleStatus;
                            }
                        }

                        if (storedPasswordHash != null && PasswordHelper.VerifyPassword(model.Password, storedPasswordHash))
                        {
                            FormsAuthentication.SetAuthCookie(model.EmpNo, false);

                            Session["EmpNo"] = model.EmpNo;
                            Session["ProgramIds"] = string.Join(",", programIds);
                            Session["EmployeeName"] = employeeName;
                            Session["ProgramRoleStatus"] = programRoleStatus;

                            Constants.sysUser = employeeName;
                            Constants.sysEmpNo = model.EmpNo;

                            return RedirectToAction("Dashboard", "Hardware");
                        }
                    }

                    ViewBag.Message = "Invalid Employee No or Password";
                    return View("UserLogin", model);
                }
                catch (SqlException sqlEx)
                {
                    ViewBag.Message = "Database connection error: " + sqlEx.Message;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "An unexpected error occurred: " + ex.Message;
                }
            }

            ViewBag.Message = "Login failed due to invalid data";
            return View("UserLogin", model);
        }




        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("UserLogin", "Account");
        }

        public ActionResult Register()
        {
            return View();
        }



        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                    {
                        string hashedPassword = PasswordHelper.HashPassword(model.Password);

                        string query = "INSERT INTO [TechAssets].[dbo].[Hardware_UserCredential] ([PasswordHash], [EmpNo], [UserType],[UserStatus]) " +
                                       "VALUES (@PasswordHash, @EmpNo, @UserType,@UserStatus)";

                        SqlCommand command = new SqlCommand(query, conn);
                        command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        command.Parameters.AddWithValue("@EmpNo", model.EmpNo);
                        command.Parameters.AddWithValue("@UserType", model.UserType);
                        command.Parameters.AddWithValue("@UserStatus", "Active");

                        conn.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            TempData["Message"] = "User registered successfully!";
                            return RedirectToAction("UserRegister");
                        }
                        else
                        {
                            TempData["Message"] = "Registration failed. Please try again.";
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    TempData["Message"] = "Database error occurred: " + sqlEx.Message;
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "An error occurred: " + ex.Message;
                }
            }
            return RedirectToAction("UserRegister", "Account");

        }






        public ActionResult UserRegister()
        {
            List<RegisterModel> users = new List<RegisterModel>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                string query = "SELECT huc.[Id], huc.[EmpNo], sed.[Name_with_ini], huc.[UserType] FROM [TechAssets].[dbo].[Hardware_UserCredential] AS huc JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS sed ON huc.[EmpNo] = sed.[Emp_no]";

                SqlCommand command = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new RegisterModel
                    {
                        EmpNo = reader["EmpNo"].ToString(),
                        UserType = reader["UserType"].ToString(),
                        UserNameI = reader["Name_with_ini"].ToString(),
                    });
                }

            }

            return View(users);
        }

        public ActionResult Test()
        {
            List<RegisterModel> users = new List<RegisterModel>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                string query = "SELECT huc.[Id], huc.[EmpNo], sed.[Name_with_ini], huc.[UserType] FROM [TechAssets].[dbo].[Hardware_UserCredential] AS huc JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS sed ON huc.[EmpNo] = sed.[Emp_no]";

                SqlCommand command = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new RegisterModel
                    {
                        EmpNo = reader["EmpNo"].ToString(),
                        UserType = reader["UserType"].ToString(),
                        UserNameI = reader["Name_with_ini"].ToString(),
                    });
                }

            }

            return View(users);
        }




        public JsonResult GetEmployeeName(string empNo)
        {
            string employeeName = null;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    string query = "SELECT a.Name_with_ini FROM [EMMSDB].[dbo].[Staff_employee_Details] AS a INNER JOIN [EMMSDB].[dbo].[Staff_Employee_Designation] AS b ON a.Emp_no = b.Emp_no WHERE b.Division = 'NRMDIV019' AND a.Emp_no = @EmpNo";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@EmpNo", empNo);

                        conn.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employeeName = reader["Name_with_ini"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(employeeName, JsonRequestBehavior.AllowGet);
        }



        public ActionResult UserRoleAssign(string empNo, string UserName, string UserType)
        {
            List<RegisterModel> roles = new List<RegisterModel>();

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {

                    string queryPrograms = "SELECT [ProgramId], [ProgramName] FROM [TechAssets].[dbo].[Hardware_Programs]";
                    SqlCommand cmdPrograms = new SqlCommand(queryPrograms, conn);
                    conn.Open();
                    SqlDataReader readerPrograms = cmdPrograms.ExecuteReader();

                    var programs = new List<RegisterModel>();
                    while (readerPrograms.Read())
                    {
                        programs.Add(new RegisterModel
                        {
                            ProgramId = readerPrograms["ProgramId"].ToString(),
                            UserName1 = readerPrograms["ProgramName"].ToString()
                        });
                    }
                    readerPrograms.Close();

                    string queryRoles = "SELECT [ProgramId], [RoleStatus] FROM [TechAssets].[dbo].[Hardware_UserRole] WHERE EmpNo = @EmpNo";
                    SqlCommand cmdRoles = new SqlCommand(queryRoles, conn);
                    cmdRoles.Parameters.AddWithValue("@EmpNo", empNo);
                    SqlDataReader readerRoles = cmdRoles.ExecuteReader();


                    var userRoles = new Dictionary<string, bool>();
                    while (readerRoles.Read())
                    {
                        userRoles[readerRoles["ProgramId"].ToString()] = Convert.ToBoolean(readerRoles["RoleStatus"]);
                    }

                    foreach (var program in programs)
                    {
                        program.RoleStatus = userRoles.ContainsKey(program.ProgramId) ? userRoles[program.ProgramId] : false;
                        roles.Add(program);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while fetching user roles: " + ex.Message;
                return View("Error");
            }

            ViewBag.EmpNo = empNo;
            ViewBag.UserName = UserName;
            ViewBag.UserType = UserType;

            return View(roles);
        }


        [HttpPost]
        public JsonResult UpdateUserRole(string empNo, string programId, bool isEnabled)
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                IF EXISTS (SELECT 1 FROM [TechAssets].[dbo].[Hardware_UserRole] WHERE EmpNo = @EmpNo AND ProgramId = @ProgramId)
                BEGIN
                    UPDATE [TechAssets].[dbo].[Hardware_UserRole]
                    SET RoleStatus = @RoleStatus
                    WHERE EmpNo = @EmpNo AND ProgramId = @ProgramId
                END
                ELSE
                BEGIN
                    INSERT INTO [TechAssets].[dbo].[Hardware_UserRole] (EmpNo, ProgramId, RoleStatus)
                    VALUES (@EmpNo, @ProgramId, @RoleStatus)
                END";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@EmpNo", empNo);
                        command.Parameters.AddWithValue("@ProgramId", programId);
                        command.Parameters.AddWithValue("@RoleStatus", isEnabled ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                string storedPasswordHash = GetStoredPasswordHash(model.EmpNo);

                if (storedPasswordHash != null && PasswordHelper.VerifyPassword(model.ExistingPassword, storedPasswordHash))
                {
                    string newPasswordHash = PasswordHelper.HashPassword(model.NewPassword);
                    UpdatePasswordInDatabase(model.EmpNo, newPasswordHash);
                    ViewBag.Message = "Password changed successfully.";
                    return RedirectToAction("UserLogin");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid existing password.");
                }
            }

            var errorMessages = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            ViewBag.ErrorMessages = errorMessages;

            return View(model);
        }



        private string GetStoredPasswordHash(string empNo)
        {
            string passwordHash = null;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                string query = "SELECT PasswordHash FROM [TechAssets].[dbo].[Hardware_UserCredential] WHERE EmpNo = @EmpNo";

                using (var command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@EmpNo", empNo);
                    conn.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        passwordHash = result.ToString();
                    }
                }
            }

            return passwordHash;
        }


        private void UpdatePasswordInDatabase(string empNo, string newPasswordHash)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                string query = "UPDATE [TechAssets].[dbo].[Hardware_UserCredential] SET PasswordHash = @NewPasswordHash WHERE EmpNo = @EmpNo";

                using (var command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);
                    command.Parameters.AddWithValue("@EmpNo", empNo);
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }
        }



        private const string DefaultPassword = "abcd@1234";


        [HttpPost]
        public JsonResult ResetPassword(string empNo)
        {
            if (string.IsNullOrEmpty(empNo))
            {
                return Json(new { success = false, message = "Employee number is required." });
            }


            string storedPasswordHash = GetStoredPasswordHash(empNo);

            if (storedPasswordHash != null)
            {
                string newPasswordHash = PasswordHelper.HashPassword(DefaultPassword);
                UpdatePasswordInDatabase(empNo, newPasswordHash);
                return Json(new { success = true, message = "Password reset successfully to the default password. Default Password - abcd@1234" });
            }
            else
            {
                return Json(new { success = false, message = "Invalid employee number." });
            }
        }





    }
}



