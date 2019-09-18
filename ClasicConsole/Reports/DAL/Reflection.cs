using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports.DAL
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Get a boolean value if type inherits from the specified base type
        /// </summary>
        /// <typeparam name="TParentType"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool InheritsFromType<TParentType>(Type type)
        {

            return typeof(TParentType).IsAssignableFrom(type);
        }

        /// <summary>
        /// Create a NULL object of the specified type
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static TObject Null<TObject>() where TObject : class
        {
            // return the NULL reference of the specified object type
            return (TObject)null;
        }

        /// <summary>
        /// Create an instance of the specified type using a default constructor
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type">Type to create an object for</param>
        /// <returns></returns>
        public static TObject CreateObject<TObject>(Type type) where TObject : class
        {
            

            object obj = Activator.CreateInstance(type);
            return (TObject)obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static TObject CreateObject<TObject>() where TObject : class
        {
            Type type = typeof(TObject);

            object obj = Activator.CreateInstance(type);

            return (TObject)obj;
        }

        /// <summary>
        /// Tries to create an object of type <see cref="Type"/> and casts result to the type of <see cref="TObject"/>
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryCreateObject<TObject>(Type type, out TObject result) where TObject : class
        {
            

            try
            {
                object obj = Activator.CreateInstance(type);
                result = (TObject)obj;
                return true;
            }
            catch
            {
                result = Null<TObject>();
                return false;
            }
        }
    }
}
