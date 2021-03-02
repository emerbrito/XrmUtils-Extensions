using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace XrmUtils.Services.Users
{
    public class TimeZoneService
    {

        private IOrganizationService orgsvc;

        public TimeZoneService(IOrganizationService organizationService)
        {
            orgsvc = organizationService;
        }

        public int? GetTimeZoneCode(Guid userId)
        {

            int? code = null;

            var query = new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("systemuserid", ConditionOperator.Equal, userId)
                    }
                }
            };

            var resp = orgsvc.RetrieveMultiple(query);

            if (resp == null || resp.Entities == null)
            {
                throw new InvalidPluginExecutionException($"Method {nameof(GetTimeZoneCode)}() returned no results.");
            }
            else if (resp.Entities.Count == 0)
            {
                throw new InvalidPluginExecutionException($"Unable to locate user settings for specified user.");
            }

            code = resp.Entities.First().GetAttributeValue<int?>("timezonecode");

            return code;
        }

        public DateTime GetLocalTimeFromUTCTime(DateTime utcTime, int timeZoneCode)
        {

            if (utcTime.Kind != DateTimeKind.Utc)
            {
                throw new InvalidPluginExecutionException($"Unexpected date kind in paramter {nameof(utcTime)}: {utcTime.Kind.ToString()}");
            }

            var req = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = timeZoneCode,
                UtcTime = utcTime
            };

            var resp = (LocalTimeFromUtcTimeResponse)orgsvc.Execute(req);

            if (resp == null)
            {
                throw new InvalidPluginExecutionException($"{nameof(LocalTimeFromUtcTimeRequest)} returned no results.");
            }

            return resp.LocalTime;

        }

        public DateTime GetUserLocalTime(Guid userId)
        {

            var code = GetTimeZoneCode(userId);
            var now = DateTime.UtcNow;

            if (!code.HasValue)
            {
                throw new InvalidPluginExecutionException($"Unable to determine time zone code for user {userId}");
            }

            return GetLocalTimeFromUTCTime(now, code.Value);

        }

        public DateTime GetUtcTimeFromLocalTime(DateTime localTime, int timeZoneCode)
        {

            var req = new UtcTimeFromLocalTimeRequest
            {
                TimeZoneCode = timeZoneCode,
                LocalTime = localTime
            };

            var resp = (UtcTimeFromLocalTimeResponse)orgsvc.Execute(req);

            if (resp == null)
            {
                throw new InvalidPluginExecutionException($"{nameof(UtcTimeFromLocalTimeRequest)} returned no results.");
            }

            return resp.UtcTime;

        }

    }
}
