using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZKelvin.CRM
{
    public static class ServiceExtensions
    {
        public static T GetCrmRecordById<T>(this System.Linq.IQueryable<T> dataset, Guid recordId, Func<T, T> selector) where T : Microsoft.Xrm.Sdk.Entity
        {
            var result = dataset
              .Where(r => r.Id == recordId)
              .Select(r => selector(r)).FirstOrDefault();

            if (result != null && result.Id == Guid.Empty)
                throw new Exception(string.Format("The primary ID of the current {0} record is not populated. Possibly the primary id field is NOT included in the selector.", result.LogicalName));

            return result;
        }
    }
}
