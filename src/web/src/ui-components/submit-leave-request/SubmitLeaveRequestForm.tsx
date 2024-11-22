import React, { useState } from "react";
import Typography from "@mui/material/Typography";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import InputLabel from "@mui/material/InputLabel";
import Select from "@mui/material/Select";
import MenuItem from "@mui/material/MenuItem";
import Box from "@mui/material/Box";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { Authorized } from "../../components/Authorized";
import { UserDto } from "../dtos/UserDto";
import { useMsal } from "@azure/msal-react";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import { DateTime, Duration } from "luxon";
import {
  Controller,
  FieldErrors,
  SubmitHandler,
  useForm,
  UseFormSetValue,
} from "react-hook-form";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid2";
import LoadingButton from "@mui/lab/LoadingButton";
import { DaysCounter } from "../utils/DaysCounter";
import { Loading } from "../../components/Loading";
import { DurationFormatter } from "../utils/DurationFormatter";
import FormHelperText from "@mui/material/FormHelperText";

const titleStyle = { color: "text.secondary" };
const defaultStyle = { paddingTop: "1px", width: "max-content" };

export interface LeaveRequestFormModel {
  dateFrom: DateTime | undefined;
  dateTo: DateTime | undefined;
  onBehalf: string | undefined;
  leaveType: string | undefined;
  remarks: string | undefined;
  workingDays: number | undefined;
  allDays: number | undefined;
  workingHours: Duration | undefined;
}

export const SubmitLeaveRequestForm = (props: {
  leaveRequests: LeaveRequestDto[] | undefined;
  holidays: HolidaysDto | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
  employees: UserDto[] | undefined;
  onSubmit: SubmitHandler<LeaveRequestFormModel>;
}) => {
  const now = DateTime.now().startOf("day");
  const getDefaultLeaveTypeId = () => {
    return props.leaveTypes?.find(() => true)?.id;
  };
  const { instance } = useMsal();
  const {
    control,
    handleSubmit,
    register,
    formState: { errors, isValid },
    setValue,
  } = useForm<LeaveRequestFormModel>({
    defaultValues: {
      dateFrom: now,
      dateTo: now,
    },
  });
  const [dateFrom, setDateFrom] = useState<DateTime | null>(now);
  const [dateTo, setDateTo] = useState<DateTime | null>(now);
  const [leaveTypeId, setLeaveTypeId] = useState<string | undefined>();
  const [submitInProgress, setSubmitInProgress] = useState(false);

  const employees = props.employees?.map(x => ({
      ...x,
      name: x.lastName ? `${x.lastName} ${x.firstName}` : x.name ?? "undefined"
    }))
    .sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? 0);
  const  currenUser = (): string | undefined => {
    const claims = instance.getActiveAccount()?.idTokenClaims;
    if(!employees) {
      return claims?.sub;
    }
    const activeUser = employees.find((x) => x.id === claims?.sub);
    const employee = activeUser ?? employees.find(() => true);
    return employee?.id;
  }

  const { ref: onBehalfRef, ...onBehalfInputProps } = register("onBehalf", {
    required: "This is required",
  });
  setValue("onBehalf", currenUser());
  const onSubmit = async (value: LeaveRequestFormModel, event?: React.BaseSyntheticEvent) => {
    if(!isValid) {
      return;
    }
    setSubmitInProgress(true);
    // If it is an error disable in progress
    if(!await props.onSubmit(value, event)) {
      setSubmitInProgress(false);
    }
  };

  const dateIsValid = (value: DateTime | null | undefined): boolean => {
    return !!value && value.isValid;
  };

  const validateDate = (value: DateTime | undefined) => {
    if (!dateIsValid(value)) return "This is required";
  };

  const onInvalid = (errors: FieldErrors<LeaveRequestFormModel>) => {
    // TODO: show notification
    alert(JSON.stringify(errors));
  }

  return (
    <form onSubmit={handleSubmit(onSubmit, onInvalid)}>
      <Container component="main" maxWidth="sm" sx={{ mb: 4 }}>
        <Paper
          variant="outlined"
          sx={{ my: { xs: 3, md: 6 }, p: { xs: 2, md: 3 } }}
        >
          <Typography component="h1" variant="h4" align="center">
            Add leave request
          </Typography>

          <Box sx={{ my: 3 }}>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12 }}>
                <Authorized
                  roles={["DecisionMaker", "GlobalAdmin"]}
                  authorized={
                    employees ? (
                      <FormControl fullWidth>
                        <InputLabel id="select-label-add-on-behalf">
                          Add on behalf of another user *
                        </InputLabel>
                        <Select
                          labelId="select-label-add-on-behalf"
                          id="select-add-on-behalf"
                          defaultValue={currenUser()}
                          label="Add on behalf of another user *"
                          inputRef={onBehalfRef}
                          {...onBehalfInputProps}
                        >
                          {employees.map((x) => (
                            <MenuItem key={`add-on-behalf-${x.id}`} value={x.id}>{x.name}</MenuItem>
                          ))}
                        </Select>
                        <FormHelperText sx={{ color: "red" }}>
                          {errors?.onBehalf?.message}
                        </FormHelperText>
                      </FormControl>
                    ) : (
                      <Loading
                        linearProgress
                        label="Add on behalf of another user *"
                      />
                    )
                  }
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  control={control}
                  name="dateFrom"
                  rules={{
                    required: "This is required",
                    validate: { required: validateDate },
                  }}
                  render={({ field }) => {
                    return (
                      <DatePicker
                        label="Date from *"
                        value={field.value}
                        inputRef={field.ref}
                        onChange={(date: DateTime | null) => {
                          setDateFrom(date);
                          field.onChange(date);
                          if (date && dateTo && date > dateTo) {
                            setValue("dateTo", date);
                            setDateTo(date);
                          }
                        }}
                        slotProps={{
                          textField: {
                            error: !dateIsValid(dateFrom),
                            helperText: errors?.dateFrom?.message,
                          },
                        }}
                      />
                    );
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller
                  control={control}
                  name="dateTo"
                  rules={{
                    required: "This is required",
                    validate: { required: validateDate },
                  }}
                  render={({ field }) => {
                    return (
                      <DatePicker
                        label="Date to *"
                        value={field.value}
                        inputRef={field.ref}
                        onChange={(date: DateTime | null) => {
                          setDateTo(date);
                          field.onChange(date);
                          if (date && dateFrom && date < dateFrom) {
                            setValue("dateFrom", date);
                            setDateFrom(date);
                          }
                        }}
                        slotProps={{
                          textField: {
                            error: !dateIsValid(dateTo),
                            helperText: errors?.dateTo?.message,
                          },
                        }}
                      />
                    );
                  }}
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                {props.leaveTypes ? (
                  <FormControl fullWidth>
                    <InputLabel id="select-label-leave-type">
                      Leave type *
                    </InputLabel>
                    <Select
                      labelId="select-label-leave-type"
                      id="select-leave-type"
                      defaultValue={getDefaultLeaveTypeId()}
                      label="Leave type *"
                      required
                      {...register("leaveType")}
                      onChange={(event) => {
                        setLeaveTypeId(event.target.value);
                      }}
                    >
                      {props.leaveTypes.map((x) => (
                        <MenuItem
                          key={`leave-types-${x.id}`}
                          value={x.id}
                          style={{
                            borderLeftColor:
                              x.properties?.color ?? "transparent",
                            borderLeftStyle: "solid",
                            borderLeftWidth: "initial",
                          }}
                        >
                          {x.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                ) : (
                  <Loading linearProgress label="Leave type *" />
                )}
              </Grid>
              <Grid size={{ xs: 12 }}>
                <TextField
                  label="Remarks"
                  fullWidth
                  rows={2}
                  multiline
                  {...register("remarks")}
                />
              </Grid>
            </Grid>
          </Box>

          <Box sx={{ my: 3 }}>
            <Typography variant="h6" gutterBottom>
              Range
            </Typography>
            {props.holidays ? (
              <Range
                holidays={props.holidays}
                dateFrom={dateFrom}
                dateTo={dateTo}
                setValue={setValue}
              />
            ) : (
              <Loading linearProgress />
            )}
          </Box>

          <Box sx={{ my: 3 }}>
            <Typography variant="h6" gutterBottom>
              Additional information
            </Typography>
            {props.leaveLimits &&
            props.leaveRequests &&
            props.holidays &&
            props.leaveTypes ? (
              <AdditionalInfo
                holidays={props.holidays}
                leaveRequests={props.leaveRequests}
                leaveLimits={props.leaveLimits}
                leaveTypes={props.leaveTypes}
                leaveTypeId={leaveTypeId ?? getDefaultLeaveTypeId() ?? ""}
                dateFrom={dateFrom}
                dateTo={dateTo}
                setValue={setValue}
              />
            ) : (
              <Loading linearProgress />
            )}
          </Box>

          <LoadingButton
            type="submit"
            variant="contained"
            sx={{ mt: 3, ml: 1 }}
            fullWidth
            disabled={!employees || !props.leaveTypes}
            loading={submitInProgress}
          >
            Submit
          </LoadingButton>
        </Paper>
      </Container>
      <input name="HiddenField2" type="hidden" value="fake" />
    </form>
  );
};

const Range = (props: {
  holidays: HolidaysDto;
  dateFrom: DateTime | null;
  dateTo: DateTime | null;
  setValue: UseFormSetValue<LeaveRequestFormModel>;
}) => {
  const daysCounter = new DaysCounter(
    props.holidays.items.map((x) => DateTime.fromISO(x))
  );
  let allDays;
  let workingDays;
  let freeDays;
  if (!props.dateFrom?.isValid) {
    allDays = workingDays = freeDays = "Date from is invalid. Please check it.";
  } else if (!props.dateTo?.isValid) {
    allDays = workingDays = freeDays = "Date to is invalid. Please check it.";
  } else {
    allDays = DaysCounter.countAllDays(props.dateFrom, props.dateTo);
    workingDays = daysCounter.workingDays(props.dateFrom, props.dateTo);
    freeDays = allDays - workingDays;
    props.setValue("workingDays", workingDays);
    props.setValue("allDays", allDays);
  }
  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Calendar days:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {allDays}
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Working days:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {workingDays}
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Free days:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {freeDays}
        </Typography>
      </Grid>
    </Grid>
  );
};

const AdditionalInfo = (props: {
  holidays: HolidaysDto;
  leaveRequests: LeaveRequestDto[];
  leaveLimits: LeaveLimitDto[];
  leaveTypes: LeaveTypeDto[];
  leaveTypeId: string;
  dateFrom: DateTime | null;
  dateTo: DateTime | null;
  setValue: UseFormSetValue<LeaveRequestFormModel>;
}) => {
  const daysUsed = props.leaveRequests
    .filter((x) => x.leaveTypeId === props.leaveTypeId)
    .map((x) => Duration.fromISO(x.duration))
    .reduce(
      (accumulator, current) => accumulator.plus(current),
      Duration.fromMillis(0)
    );

  let availableDaysStr =
    "There is no limit for this user, leave type or in that period";
  let availableDaysAfterAcceptanceStr = availableDaysStr;
  let currentLimit;
  if (!props.dateFrom?.isValid) {
    availableDaysStr = availableDaysAfterAcceptanceStr =
      "Date from is invalid. Please check it.";
  } else if (!props.dateTo?.isValid) {
    availableDaysStr = availableDaysAfterAcceptanceStr =
      "Date to is invalid. Please check it.";
  } else {
    const dateFrom = props.dateFrom;
    const dateTo = props.dateTo;
    currentLimit = props.leaveLimits.find(
      (x) =>
        x.state === "Active" &&
        x.leaveTypeId === props.leaveTypeId &&
        (!x.validSince || DateTime.fromISO(x.validSince) <= dateFrom) &&
        (!x.validUntil || DateTime.fromISO(x.validUntil) >= dateTo)
    );
    if (currentLimit) {
      const limit = currentLimit.limit
        ? Duration.fromISO(currentLimit.limit)
        : undefined;
      const overdueLimit = currentLimit.overdueLimit
        ? Duration.fromISO(currentLimit?.overdueLimit)
        : undefined;
      const limitSum = overdueLimit ? limit?.plus(overdueLimit) : limit;
      const daysCounter = DaysCounter.create(
        props.leaveTypeId,
        props.leaveTypes,
        props.holidays.items.map((x) => DateTime.fromISO(x))
      );
      const availableDays = limitSum?.minus(daysUsed);
      const currentRequestDays = daysCounter.days(dateFrom, dateTo);
      const workingHours = currentLimit
        ? Duration.fromISO(currentLimit.workingHours)
        : findWorkingHoursAndParseDuration();
      props.setValue("workingHours", workingHours);
      const currentRequestDaysDuration = Duration.fromObject({
        hours: currentRequestDays * (workingHours?.as("hours") ?? 8),
      });
      availableDaysStr = availableDays
        ? DurationFormatter.format(availableDays, currentLimit.workingHours)
        : "There is no limit";
      availableDaysAfterAcceptanceStr = availableDays
        ? DurationFormatter.format(
            availableDays.minus(currentRequestDaysDuration),
            currentLimit.workingHours
          )
        : "There is no limit";
    }
  }
  return (
    <Grid container spacing={3}>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Available days:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {availableDaysStr}
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Days available after request acceptance:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {availableDaysAfterAcceptanceStr}
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 4 }}>
        <Typography variant="body1" sx={titleStyle}>
          Days used:
        </Typography>
      </Grid>
      <Grid size={{ xs: 12, sm: 8 }}>
        <Typography variant="body2" sx={defaultStyle}>
          {DurationFormatter.format(
            daysUsed,
            currentLimit?.workingHours ?? findWorkingHours()
          )}
        </Typography>
      </Grid>
    </Grid>
  );

  function findWorkingHoursAndParseDuration(): Duration | undefined {
    const workingHours = findWorkingHours();
    return workingHours ? Duration.fromISO(workingHours) : undefined;
  }
  function findWorkingHours(): string | undefined {
    return props.leaveRequests.find((x) => x.leaveTypeId === props.leaveTypeId)
      ?.workingHours;
  }
};
