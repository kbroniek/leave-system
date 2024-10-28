import React from "react";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import FormControl from "@mui/material/FormControl";
import TextField from "@mui/material/TextField";
import InputLabel from "@mui/material/InputLabel";
import Select, { SelectChangeEvent } from "@mui/material/Select";
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
import { Controller, useForm } from 'react-hook-form';
import Container from "@mui/material/Container";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid2";
import FormLabel from "@mui/material/FormLabel";
import RadioGroup from "@mui/material/RadioGroup";
import FormControlLabel from "@mui/material/FormControlLabel";
import Radio from "@mui/material/Radio";
import FormGroup from "@mui/material/FormGroup";
import Checkbox from "@mui/material/Checkbox";
import Button from "@mui/material/Button";

export const SubmitLeaveRequestForm = (props: {
  leaveRequests: LeaveRequestsResponseDto;
  holidays: HolidaysDto;
  leaveTypes: LeaveTypeDto[];
  leaveLimits: LeaveLimitDto[];
  employees: UserDto[];
}) => {
  const { instance } = useMsal();
  const claims = instance.getActiveAccount()?.idTokenClaims;
  const activeUser = props.employees.find(x => x.id === claims?.sub);
  const [employee, setApiEmployee] = React.useState(activeUser ?? props.employees.find(() => true));

  const handleChange = (event: SelectChangeEvent) => {
    setApiEmployee(props.employees.find(x => x.id === event.target.value));
  };
  const { control, handleSubmit, register } = useForm();

  const onSubmit = (value:unknown) => {
    alert(JSON.stringify(value));
  };
  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <React.Fragment>
        <Container component='main' maxWidth='sm' sx={{ mb: 4 }}>
          <Paper
            variant='outlined'
            sx={{ my: { xs: 3, md: 6 }, p: { xs: 2, md: 3 } }}
          >
            <Typography component='h1' variant='h4' align='center'>
              Add leave request
            </Typography>

            <Box sx={{ my: 3 }}>
              <Grid container spacing={3}>
                <Grid size={{ xs: 12}}>
                <Authorized roles={["DecisionMaker", "GlobalAdmin"]} authorized={
                  <FormControl fullWidth>
                    <InputLabel id="select-label-add-on-behalf">Add on behalf of another user</InputLabel>
                    <Select
                      labelId="select-label-add-on-behalf"
                      id="select-add-on-behalf"
                      value={employee?.id}
                      label="Add on behalf of another user"
                      required
                      {...register('onBehalf')}
                    >
                      {props.employees.map(x => (
                        <MenuItem value={x.id}>{x.name}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>}
                />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Controller
                    control={control}
                    name="dateFrom"
                    rules={{ required: true }}
                    render={({ field }) => {
                      return (
                        <DatePicker
                          label="Date from"
                          defaultValue={DateTime.now()}
                          value={field.value}
                          inputRef={field.ref}
                          onChange={(date) => {
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
                    render={({ field }) => {
                      return (
                        <DatePicker
                          label="Date to"
                          defaultValue={DateTime.now()}
                          value={field.value}
                          inputRef={field.ref}
                          onChange={(date) => {
                            field.onChange(date);
                          }}
                        />
                      );
                    }}
                  />
                </Grid>
                <Grid size={{ xs: 12}}>
                  <TextField
                    required
                    label='Address line'
                    fullWidth
                    variant='standard'
                    {...register('address')}
                  />
                </Grid>
                <Grid size={{ xs: 12}}>
                  <FormLabel sx={{ textAlign: 'left' }}>Country</FormLabel>
                  <Select
                    required
                    label='Country'
                    fullWidth
                    variant='standard'
                    {...register('country')}
                  >
                    <MenuItem value='USA'>USA</MenuItem>
                    <MenuItem value='America'>America</MenuItem>
                    <MenuItem value='Nigeria'>Nigeria</MenuItem>
                  </Select>
                </Grid>
              </Grid>
            </Box>

            <Box sx={{ my: 3 }}>
              <Typography variant='h6' gutterBottom>
                Payment method
              </Typography>
              <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    required
                    label='Name on card'
                    fullWidth
                    variant='standard'
                    {...register('cardName')}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    required
                    label='Card number'
                    fullWidth
                    variant='standard'
                    {...register('cardNumber')}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    required
                    label='Expiry date'
                    fullWidth
                    variant='standard'
                    {...register('expDate')}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    required
                    label='CVV'
                    helperText='Last three digits on signature strip'
                    fullWidth
                    variant='standard'
                    {...register('cvv')}
                  />
                </Grid>
              </Grid>
            </Box>

            <Box>
              <RadioGroup
                defaultValue='payCard'
                row
                {...register('paymentType')}
              >
                <FormControlLabel
                  value='payCard'
                  control={<Radio />}
                  label='Pay by Card'
                />
                <FormControlLabel
                  value='payTransfer'
                  control={<Radio />}
                  label='Pay by Transfer'
                />
              </RadioGroup>
              <FormGroup>
                <FormControlLabel
                  control={
                    <Checkbox defaultChecked {...register('saveForLater')} />
                  }
                  label='Save the information for later'
                />
              </FormGroup>
            </Box>

            <Button
              type='submit'
              variant='contained'
              sx={{ mt: 3, ml: 1 }}
              fullWidth
            >
              Purchase
            </Button>
          </Paper>
        </Container>
      </React.Fragment>
    </form>
  );
};
