using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HuntersWP.Models;

namespace HuntersWP.Services
{
    public class ApiResponseProcessor
    {
        public static bool Execute<T>(ApiResponse<T> response)
        {
            if (response.IsSuccess)
                return true;

            MessageBox.Show(response.Message ?? "Error");

            return false;


        }
    }
}
