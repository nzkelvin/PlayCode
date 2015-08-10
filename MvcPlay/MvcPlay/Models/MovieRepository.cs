using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcPlay.Models
{
    public class MovieRepository
    {
        public IEnumerable<Movie> GetAll()
        {
            return new []{
              new Movie {Title="Fantastic Four", Rating=2}
            };
        }
    }
}