using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HuntersWP.Models;
using HuntersWP.ServiceReference;

namespace HuntersWP.Services
{
    public class ApiResponseProcessor
    {
        public static bool Execute(BaseReply response)
        {
            if (response.IsSuccess)
                return true;

            MessageBox.Show(response.Data ?? "Error occurred");

            return false;


        }
    }
}
