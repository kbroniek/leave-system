import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { LoadingAuth } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import {
  callApi,
  callApiGet,
  ifErrorAcquireTokenRedirect,
} from "../../utils/ApiCall";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveRequestsResponseDto } from "../leave-requests/LeaveRequestsDto";
import {
  LeaveRequestFormModel,
  SubmitLeaveRequestForm,
} from "./SubmitLeaveRequestForm";
import { DateTime, Duration } from "luxon";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { EmployeesDto } from "../dtos/EmployeesDto";
import { v4 as uuidv4 } from "uuid";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] = useState<
    LeaveRequestsResponseDto | undefined
  >();
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | undefined>();
  const [apiLeaveTypes, setApiLeaveTypes] = useState<
    LeaveTypesDto | undefined
  >();
  const [apiLeaveLimits, setApiLeaveLimits] = useState<
    LeaveLimitsDto | undefined
  >();
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | undefined>();
  const navigate = useNavigate();

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      const userId = instance.getActiveAccount()?.idTokenClaims?.sub;
      const now = DateTime.local();
      const currentYear = now.toFormat("yyyy");
      const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
      const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}&AssignedToUserIds=${userId}`
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveTypesDto>("/leavetypes")
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));

      callApiGet<HolidaysDto>(
        `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`
      )
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveLimitsDto>(`/leavelimits/user?year=${currentYear}`)
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<EmployeesDto>("/employees")
        .then((response) => setApiEmployees(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequests, instance]);

  const onSubmit = async (model: LeaveRequestFormModel) => {
    if (!model.dateFrom?.isValid) {
      //TODO: show notification
      alert("Date from is invalid");
      return;
    }
    if (!model.dateTo?.isValid) {
      //TODO: show notification
      alert("Date to is invalid");
      return;
    }
    if (!model.leaveType) {
      //TODO: show notification
      alert("Leave type is invalid");
      return;
    }
    if (!model.allDays) {
      //TODO: show notification
      alert("Form is invalid. Can't read all days.");
      return;
    }
    if (!model.workingDays) {
      //TODO: show notification
      alert("Form is invalid. Can't read working days.");
      return;
    }
    if (!model.workingHours) {
      //TODO: show notification
      alert("Form is invalid. Can't read working hours.");
      return;
    }
    if (!apiLeaveTypes) {
      //TODO: show notification
      alert("Form is invalid. Can't read leave types.");
      return;
    }
    const calculateDuration = (): Duration => {
      const leaveType = apiLeaveTypes.items.find(
        (x) => x.id === model.leaveType
      );
      const duration =
        leaveType?.properties?.includeFreeDays ?? true
          ? model.workingHours!.as("milliseconds") * model.allDays!
          : model.workingHours!.as("milliseconds") * model.workingDays!;

      return Duration.fromObject({
        days: 0,
        hours: 0,
        seconds: 0,
        milliseconds: duration,
      }).normalize();
    };
    const isNotOnBehalf =
      !model.onBehalf ||
      model.onBehalf === instance.getActiveAccount()?.localAccountId;
    const url = isNotOnBehalf ? "/leaverequests" : "/leaverequests/onbehalf";
    const body = {
      leaveRequestId: uuidv4(),
      dateFrom: model.dateFrom.toFormat("yyyy-MM-dd"),
      dateTo: model.dateTo.toFormat("yyyy-MM-dd"),
      duration: calculateDuration().toISO(),
      workingHours: model.workingHours.toISO(),
      leaveTypeId: model.leaveType,
      remark: model.remarks,
      assignedToId: isNotOnBehalf ? undefined : model.onBehalf,
    };
    const response = await callApi(url, "POST", body);
    if(response.status === 201) {
      navigate("/")
      return response.status;
    }
    else {
      //TODO: show notification
      const errorBody = await response.json();
      alert(`Error: ${response.status}
        ${errorBody.title}
        ${errorBody.detail}
        ${response.statusText}`);
    }
  };

  return (
    <Authorized
      roles={["DecisionMaker", "GlobalAdmin", "Employee"]}
      authorized={(
        <SubmitLeaveRequestForm
          leaveRequests={apiLeaveRequests?.items}
          holidays={apiHolidays}
          leaveTypes={apiLeaveTypes?.items}
          leaveLimits={apiLeaveLimits?.items}
          employees={apiEmployees?.items}
          onSubmit={onSubmit}
        />
      )}
      unauthorized={(<Forbidden />)}
    />
  );
};

export function SubmitLeaveRequest() {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <DataContent />
    </MsalAuthenticationTemplate>
  );
}
