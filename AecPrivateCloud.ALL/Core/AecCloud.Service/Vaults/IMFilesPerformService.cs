using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.Service.Vaults
{
    public interface IMFilesPerformService
    {
        List<PerformanceRateModel> GetPerformRate(List<PerformanceRateModel> list,Dictionary<string,string> guidAndIps, string username, string pwd, int year, int month);

        List<UnitPerformaceModel> GetPerformRateUnit(Dictionary<string, string> guidAndIps, string username, string pwd, int year, int month, string compName);
    }
}
