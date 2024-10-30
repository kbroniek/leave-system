import React, { useState } from "react";
import Typography from "@mui/material/Typography";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import InputLabel from "@mui/material/InputLabel";
import Select from "@mui/material/Select";
import MenuItem from "@mui/material/MenuItem";
import Box from "@mui/material/Box";
import { LeaveRequestsResponseDto } from "../leave-requests/LeaveRequestsDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { Authorized } from "../../components/Authorized";
import { UserDto } from "../dtos/UserDto";
import { useMsal } from "@azure/msal-react";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import { DateTime } from "luxon";
import { Controller, useForm } from "react-hook-form";
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid2";
import Button from "@mui/material/Button";
import { DaysCounter } from "../utils/DaysCounter";
import { Loading } from "../Loading";


const titleStyle = { color: "text.secondary" };
const defaultStyle = { paddingTop: "1px", width: "max-content" };

export const SubmitLeaveRequestForm = (props: {
  leaveRequests: LeaveRequestsResponseDto | undefined;
  holidays: HolidaysDto | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
  employees: UserDto[] | undefined;
}) => {
  const { instance } = useMsal();
  const { control, handleSubmit, register } = useForm();
  const now = DateTime.now();
  const [dateFrom, setDateFrom] = useState(now);
  const [dateTo, setDateTo] = useState(now);
  
  const onSubmit = (value: unknown) => {
    alert(JSON.stringify(value));
  };

  function currenUser(employees: UserDto[]): string | undefined {
    const claims = instance.getActiveAccount()?.idTokenClaims;
    const activeUser = employees.find((x) => x.id === claims?.sub);
    const employee = activeUser ?? employees.find(() => true);
    return employee?.id;
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <React.Fragment>
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
                      props.employees ? (
                        <FormControl fullWidth>
                          <InputLabel id="select-label-add-on-behalf">
                            Add on behalf of another user *
                          </InputLabel>
                          <Select
                            labelId="select-label-add-on-behalf"
                            id="select-add-on-behalf"
                            defaultValue={currenUser(props.employees)}
                            label="Add on behalf of another user *"
                            required
                            {...register("onBehalf")}
                          >
                            {props.employees.map((x) => (
                              <MenuItem value={x.id}>{x.name}</MenuItem>
                            ))}
                          </Select>
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
                    rules={{ required: true }}
                    defaultValue={dateFrom}
                    render={({ field }) => {
                      return (
                        <DatePicker
                          label="Date from *"
                          defaultValue={dateFrom}
                          value={field.value}
                          inputRef={field.ref}
                          onChange={(date) => {
                            setDateFrom(date);
                            field.onChange(date);
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
                    rules={{ required: true }}
                    defaultValue={dateTo}
                    render={({ field }) => {
                      return (
                        <DatePicker
                          label="Date to *"
                          defaultValue={dateTo}
                          value={field.value}
                          inputRef={field.ref}
                          onChange={(date) => {
                            setDateTo(date);
                            field.onChange(date);
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
                        defaultValue={props.leaveTypes.find(() => true)?.id}
                        label="Leave type *"
                        required
                        {...register("leaveType")}
                      >
                        {props.leaveTypes.map((x) => (
                          <MenuItem
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
                    <Loading
                      linearProgress
                      label="Leave type *"
                    />
                  )}
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    label="Remarks"
                    fullWidth
                    rows={2}
                    multiline
                    {...register("cardName")}
                  />
                </Grid>
              </Grid>
            </Box>

            <Box sx={{ my: 3 }}>
              <Typography variant="h6" gutterBottom>
                Range
              </Typography>
              {props.holidays ? (<Range holidays={props.holidays} dateFrom={dateFrom} dateTo={dateTo} />) : (
                <Loading
                  linearProgress
                />)
              }
            </Box>

            <Box sx={{ my: 3 }}>
              <Typography variant="h6" gutterBottom>
                Additional information
              </Typography>
              {props.leaveLimits ? (<AdditionalInfo />) : (
                <Loading
                  linearProgress
                />)
              }
            </Box>

            <Button
              type="submit"
              variant="contained"
              sx={{ mt: 3, ml: 1 }}
              fullWidth
              disabled={!props.employees || !props.leaveTypes}
            >
              Submit
            </Button>
          </Paper>
        </Container>
      </React.Fragment>
    </form>
  );
};

const Range = (props: {holidays: HolidaysDto, dateFrom: DateTime, dateTo: DateTime}) => {
  const daysCounter = new DaysCounter(
    props.holidays.items.map((x) => DateTime.fromISO(x))
  );
  const allDays = DaysCounter.countAllDays(props.dateFrom, props.dateTo);
  const workingDays = daysCounter.workingDays(props.dateFrom, props.dateTo);
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
          {allDays - workingDays}
        </Typography>
      </Grid>
    </Grid>);
}

const AdditionalInfo = () => {
  return <Grid container spacing={3}>
  <Grid size={{ xs: 12, sm: 4 }}>
    <Typography variant="body1" sx={titleStyle}>
      Available days:
    </Typography>
  </Grid>
  <Grid size={{ xs: 12, sm: 8 }}>
    <Typography variant="body2" sx={defaultStyle}>
      TODO
    </Typography>
  </Grid>
  <Grid size={{ xs: 12, sm: 4 }}>
    <Typography variant="body1" sx={titleStyle}>
      Days available after request acceptance:
    </Typography>
  </Grid>
  <Grid size={{ xs: 12, sm: 8 }}>
    <Typography variant="body2" sx={defaultStyle}>
      TODO
    </Typography>
  </Grid>
  <Grid size={{ xs: 12, sm: 4 }}>
    <Typography variant="body1" sx={titleStyle}>
      Free days:
    </Typography>
  </Grid>
  <Grid size={{ xs: 12, sm: 8 }}>
    <Typography variant="body2" sx={defaultStyle}>
      TODO
    </Typography>
  </Grid>
</Grid>
}