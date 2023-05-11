using HeyDoc.Web.Lib;
using HeyDoc.Web.Models;
using HeyDoc.Web.Services;
using HeyDoc.Web.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace HeyDoc.Web.Controllers.Api
{
    public class PrescriptionController : ApiController
    {
        [HttpPost]
        public Task<PrescriptionModel> Create([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, PrescriptionModel model)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.CreatePrescription(accessToken, chatRoomId, model));
        }

        [HttpPost]
        public BoolResult Dispense([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId)
        {
            return WebApiWrapper.Call<BoolResult>(e => PrescriptionService.MarkDispensed(accessToken, prescriptionId,true));
        }

        [HttpPost]
        public Task<BoolResult> DispenseMedication([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, DispensePrescriptionModel model)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.MarkPrescriptionDispensed(accessToken, prescriptionId, model));
        }

        [HttpGet]
        public List<PrescriptionModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatRoomId, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<PrescriptionModel>>(e => PrescriptionService.GetPrescriptionList(accessToken, chatRoomId, take, skip));
        }
        
        [HttpGet]
        public List<PrescriptionModel> List([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int take = 15, int skip = 0)
        {
            return WebApiWrapper.Call<List<PrescriptionModel>>(e => PrescriptionService.GetPrescriptionList(accessToken, take, skip));
        }

        [HttpGet]
        public List<PrescriptionModel> GetPrescriptionList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, PrescriptionAvailabilityStatus? status, int take = 15, int skip = 0)
        {
           return WebApiWrapper.Call<List<PrescriptionModel>>(e => PrescriptionService.GetPrescriptionListByStatus(accessToken, status, take, skip));           
        }

        [HttpPost]
        public Task<BoolResult> MarkPrescriptionStatus([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, DoctorRemarkModel model)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.MarkPrescriptionByStatus(accessToken, prescriptionId, model));
        }

        [HttpGet]
        public PrescriptionModel Prescription([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken,long prescriptionId)
        {
            return WebApiWrapper.Call<PrescriptionModel>(e => PrescriptionService.GetPrescriptionById(accessToken, prescriptionId));
        }

        [HttpDelete]
        public BoolResult Delete([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId)
        {
            return WebApiWrapper.Call<BoolResult>(e => PrescriptionService.DeletePrescription(accessToken, prescriptionId));
        }

        [HttpPost]
        public PrescriptionModel RequestMedication([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, PrescriptionStatus prescriptionStatus, int? Id = null, string deliveryAddress = "")
        {
            return WebApiWrapper.Call<PrescriptionModel>(e => PrescriptionService.RequestMedication(accessToken, prescriptionId, prescriptionStatus, Id , deliveryAddress));
        }

        [HttpPost]
        public Task<PrescriptionModel> RequestMedicationDelivery([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, string deliveryAddress, PackingType packingType = PackingType.NotSpecified)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.RequestMedicationDelivery(accessToken, prescriptionId, deliveryAddress, packingType));
        }

        [HttpPost]
        public Task<PrescriptionModel> RequestMedicationSelfCollection([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, int Id, PackingType packingType = PackingType.NotSpecified)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.RequestMedicationSelfCollection(accessToken, prescriptionId, Id, packingType));
        }

        [HttpPost]
        public Task<PrescriptionModel> RequestMedicationOnsite([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId, int Id, PackingType packingType = PackingType.NotSpecified)
        {
            return WebApiWrapper.CallAsync(() => PrescriptionService.RequestMedicationOnsite(accessToken, prescriptionId, Id, packingType));
        }

        [HttpGet]
        public PrescriptionModel GetPrescriptionrecord([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId)
        {
            return WebApiWrapper.Call<PrescriptionModel>(e => PrescriptionService.PrescriptionRecord(accessToken, prescriptionId));
        }

        [HttpPost]
        public BoolResult CancelPrescription([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId)
        {
            return WebApiWrapper.Call<BoolResult>(e => PrescriptionService.CancelPrescription(accessToken, prescriptionId));
        }

        [HttpPost]
        public BoolResult AssignPrescriptionToSelf([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, long prescriptionId)
        {
            return WebApiWrapper.Call(e => new BoolResult(PrescriptionService.AssignPrescriptionToSelf(accessToken, prescriptionId)));
        }

        [HttpGet]
        public List<PrescriptionModel> GetUnassignedPrescriptionList([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int skip = 0, int take = 15)
        {
            return WebApiWrapper.Call(e => PrescriptionService.GetUnassignedPrescriptionList(accessToken, skip, take));
        }

        [HttpGet]
        public BoolResultWithData<List<string>> CheckChatroomPatientMissingInfo([ValueProvider(new[] { typeof(HeaderValueProviderFactory), typeof(QueryStringValueProviderFactory) })] string accessToken, int chatroomId)
        {
            return WebApiWrapper.Call(e =>
            {
                (bool isInfoComplete, List<string> missingFields) = PrescriptionService.CheckChatroomMissingInfoForPrescription(accessToken, chatroomId);
                return new BoolResultWithData<List<string>>(isInfoComplete, missingFields);
            });
        }
    }
}
