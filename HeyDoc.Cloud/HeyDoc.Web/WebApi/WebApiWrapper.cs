using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HeyDoc.Web.WebApi
{
    public class WebApiWrapper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public delegate T GenericWebApi<T>(params object[] inputs);

        public static TResult Call<TResult>(GenericWebApi<TResult> func, params object[] inputs)
        {
            try
            {
                return func(inputs);
            }
            //catch (Exception ex)
            //{
            //    if (ex is WebApiException)
            //    {
            //        throw;
            //    }
            //    else
            //    {
            //        log.Error(ex);
            //        throw new WebApiException(new WebApiError(ex));
            //    }

            //}
            //Uncomment below and comment above if you want to see errors
            catch (Exception ex)
            {

                log.Error(ex);
                //throw new WebApiException(new WebApiError(ex));
                throw ex;

            }
        }

        public static void Call(Action func)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                if (ex is WebApiException)
                {
                    throw;
                }
                else
                {
                    log.Error(ex);
                    throw new WebApiException(new WebApiError(ex));
                }
            }
        }

        public static Task<TResult> CallIntoTask<TResult>(GenericWebApi<TResult> func, params object[] inputs)
        {
            try
            {
                return Task.FromResult(func());
            }
            catch (Exception ex)
            {
                if (ex is WebApiException)
                {
                    throw ex;
                }
                else
                {
                    log.Error(ex);
                    throw new WebApiException(new WebApiError(ex));
                }
            }
        }

        public static async Task<TResult> CallAsync<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                if (ex is WebApiException)
                {
                    throw;
                }
                else
                {
                    log.Error(ex);
                    throw new WebApiException(new WebApiError(ex));
                }
            }
        }

        public static async Task CallAsync(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                if (ex is WebApiException)
                {
                    throw;
                }
                else
                {
                    log.Error(ex);
                    throw new WebApiException(new WebApiError(ex));
                }
            }
        }
    }
}