using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Fundamentals
{
    public static class LambdaExpression
    {
        public static void Run()
        {
            Console.WriteLine(TellMeTheProperty<User>(u => u.Id));
            Console.WriteLine(TellMeTheProperty<User>(u => u.Name));
        }

        private static string TellMeTheProperty<T>(Expression<Func<T, object>> expression)
        {
            var member = expression.Body as MemberExpression;
            var unary = expression.Body as UnaryExpression;
            return member?.Member.Name ?? (unary.Operand as MemberExpression).Member.Name;
        }
    }

    public class User
    {
        //public User(string name)
        //{
        //    Name = name;
        //    Id = 1;
        //}

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
