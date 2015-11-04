using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conf.DataModel.Sql
{
    /// <summary>
    /// Model from https://msignite.nz/sessions
    /// </summary>
    public class ConfDataContext: DbContext
    {
        public DbSet<Conf.Model.Session> Sessions { get; set; }
    }
}
