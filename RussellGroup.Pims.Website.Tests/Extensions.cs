using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RussellGroup.Pims.Website.Tests
{
    public static class Extensions
    {
        public static string GetFirstErrorMessage(this ViewResult result)
        {
            var state = result.ViewData.ModelState;
            var values = state.Values.FirstOrDefault();

            if (values != null)
            {
                var errors = values.Errors;

                if (errors.Any())
                {
                    return errors[0].ErrorMessage;
                }

                throw new ArgumentException("There are no errors of the view result.");
            }

            throw new ArgumentException("There are no values of the view result.");
        }

        public static BatchCheckout GetBatchCheckoutModel(this ViewResult result)
        {
            if (result.Model is BatchCheckout)
            {
                return result.Model as BatchCheckout;
            }

            throw new ArgumentException("The model of the view result is not a BatchCheckout class.");
        }

        public static CheckoutTransaction GetFirstBatchCheckoutTransaction(this ViewResult result)
        {
            return result.GetBatchCheckoutModel().CheckoutTransactions.FirstOrDefault();
        }

        public static Plant[] GetPlantsOfFirstBatchCheckoutTransaction(this ViewResult result)
        {
            return result.GetFirstBatchCheckoutTransaction().Plants.ToArray();
        }

        public static BatchCheckin GetBatchCheckinModel(this ViewResult result)
        {
            if (result.Model is BatchCheckin)
            {
                return result.Model as BatchCheckin;
            }

            throw new ArgumentException("The model of the view result is not a BatchCheckin class.");
        }

        public static CheckinTransaction GetFirstBatchCheckinTransaction(this ViewResult result)
        {
            return result.GetBatchCheckinModel().CheckinTransactions.FirstOrDefault();
        }

        public static Plant[] GetPlantHiresOfFirstBatchCheckinTransaction(this ViewResult result)
        {
            return result.GetFirstBatchCheckinTransaction().PlantHires.Select(f => f.Plant).ToArray();
        }
    }
}
