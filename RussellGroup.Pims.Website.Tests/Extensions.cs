﻿using RussellGroup.Pims.DataAccess.Models;
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
        // http://stackoverflow.com/questions/30671033/unit-test-the-bindattribute-for-method-parameters
        public static void BindModel<TModel, TController>(this TController controller, TModel model, string methodName)
        {
            foreach (var method in typeof(TController).GetMethods().Where(x => x.Name.Equals(methodName)))
            {
                foreach (var parameter in method.GetParameters())
                {
                    foreach (BindAttribute bindAttribute in parameter.GetCustomAttributes(true))
                    {
                        var propertiesToReset = typeof(TModel).GetProperties().Where(x => bindAttribute.IsPropertyAllowed(x.Name) == false).Where(x => x.CanWrite);

                        foreach (var propertyToReset in propertiesToReset)
                        {
                            propertyToReset.SetValue(model, null);
                        }
                    }
                }
            }
        }

        public static string GetErrorMessage(this ViewResult result, int elementNumber = 0)
        {
            var state = result.ViewData.ModelState;
            var values = state.Values.ToArray()[elementNumber];

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

        public static CheckoutTransaction GetBatchCheckoutTransaction(this ViewResult result, int elementNumber = 0)
        {
            return result.GetBatchCheckoutModel().CheckoutTransactions.Skip(elementNumber).FirstOrDefault();
        }

        public static Plant[] GetPlantsOfBatchCheckoutTransaction(this ViewResult result, int elementNumber = 0)
        {
            return result.GetBatchCheckoutTransaction(elementNumber).Plants.ToArray();
        }

        public static BatchCheckin GetBatchCheckinModel(this ViewResult result)
        {
            if (result.Model is BatchCheckin)
            {
                return result.Model as BatchCheckin;
            }

            throw new ArgumentException("The model of the view result is not a BatchCheckin class.");
        }

        public static CheckinTransaction GetBatchCheckinTransaction(this ViewResult result, int elementNumber = 0)
        {
            return result.GetBatchCheckinModel().CheckinTransactions.Skip(elementNumber).FirstOrDefault();
        }

        public static Plant[] GetPlantsOfBatchCheckinTransaction(this ViewResult result, int elementNumber = 0)
        {
            return result.GetBatchCheckinTransaction(elementNumber).PlantHires.Select(f => f.Plant).ToArray();
        }

        public static BatchStatus GetBatchStatusModel(this ViewResult result)
        {
            if (result.Model is BatchStatus)
            {
                return result.Model as BatchStatus;
            }

            throw new ArgumentException("The model of the view result is not a BatchStatus class.");
        }

        public static Plant[] GetPlantsOfBatchStatus(this ViewResult result)
        {
            return result.GetBatchStatusModel().Plants.ToArray();
        }
    }
}
