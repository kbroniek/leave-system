import Grid from "@mui/material/Grid2";
import { DatePicker } from "@mui/x-date-pickers";
import { DateTime } from "luxon";
import { useState } from "react";
import { Controller, SubmitHandler, useForm } from "react-hook-form";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import InputLabel from "@mui/material/InputLabel";
import Select, { SelectChangeEvent } from "@mui/material/Select";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import MenuItem from "@mui/material/MenuItem";
import { Theme, useTheme } from "@mui/material/styles";
import FormControl from "@mui/material/FormControl";
import { EmployeeDto } from "../dtos/EmployeeDto";
import Button from "@mui/material/Button";

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
};

export const LeaveRequestsSearch = (
  params: Readonly<{
    leaveTypes: LeaveTypeDto[] | undefined;
    employees: EmployeeDto[];
    onSubmit: SubmitHandler<SearchLeaveRequestModel>;
  }>,
) => {
  const theme = useTheme();
  const now = DateTime.now().startOf("day");
  const {
    control,
    handleSubmit,
    register,
    formState: { errors, isValid },
    setValue,
  } = useForm<SearchLeaveRequestModel>({
    defaultValues: {
      dateFrom: now.minus({ days: 14 }),
      dateTo: now.plus({ days: 14 }),
    },
  });
  const [dateFrom, setDateFrom] = useState<DateTime | null>(
    now.minus({ days: 14 }),
  );
  const [dateTo, setDateTo] = useState<DateTime | null>(now.plus({ days: 14 }));
  const [leaveTypes, setLeaveTypes] = useState<string[] | undefined>([]);
  const [employees, setEmployees] = useState<string[] | undefined>([]);
  const dateIsValid = (value: DateTime | null | undefined): boolean => {
    return !!value && value.isValid;
  };
  const validateDate = (value: DateTime | undefined) => {
    if (!dateIsValid(value)) return "This is required";
  };
  const handleLeaveTypesChange = (
    event: SelectChangeEvent<typeof leaveTypes>,
  ) => {
    const {
      target: { value },
    } = event;
    // On autofill we get a stringified value.
    const values = typeof value === "string" ? value.split(",") : value;
    setLeaveTypes(values);
    setValue("leaveTypes", values ?? []);
  };
  const handleEmployeesChange = (
    event: SelectChangeEvent<typeof employees>,
  ) => {
    const {
      target: { value },
    } = event;
    // On autofill we get a stringified value.
    const values = typeof value === "string" ? value.split(",") : value;
    setEmployees(values);
    setValue("employees", values ?? []);
  };
  function getStyles(
    name: string,
    personName: readonly string[],
    theme: Theme,
  ) {
    return {
      fontWeight: personName.includes(name)
        ? theme.typography.fontWeightMedium
        : theme.typography.fontWeightRegular,
    };
  }
  const onSearch = async (value: SearchLeaveRequestModel, event?: React.BaseSyntheticEvent) => {
    if(!isValid) {
      return;
    }
    value.employees = typeof value.employees === "string" ? (value.employees as string).split(",") : value.employees;
    value.leaveTypes = typeof value.leaveTypes === "string" ? (value.leaveTypes as string).split(",") : value.leaveTypes;
    await params.onSubmit(value, event)
  };
  return (
    <form onSubmit={handleSubmit(onSearch)}>
      <Grid container spacing={1}>
        <Grid size={{ xs: 12, sm: 3, md: 2 }}>
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
        <Grid size={{ xs: 12, sm: 3, md: 2 }}>
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
        <Grid size={{ xs: 12, sm: 3, md: 3 }}>
          <FormControl fullWidth>
            <InputLabel id="demo-multiple-leave-types-label">
              Leave types
            </InputLabel>
            <Select
              labelId="demo-multiple-leave-types-label"
              label="Leave types"
              id="multiple-leave-types"
              multiple
              sx={{ width: "100%" }}
              value={leaveTypes}
              {...register("leaveTypes")}
              onChange={handleLeaveTypesChange}
              renderValue={(selected) => (
                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                  {selected.map((value) => (
                    <Chip
                      key={value}
                      label={
                        params.leaveTypes?.find((x) => x.id === value)?.name ??
                        value
                      }
                    />
                  ))}
                </Box>
              )}
              MenuProps={MenuProps}
            >
              {params.leaveTypes?.map((item) => (
                <MenuItem
                  key={item.id}
                  value={item.id}
                  style={getStyles(item.id, leaveTypes ?? [], theme)}
                >
                  {item.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 3, md: 3 }}>
          <FormControl fullWidth>
            <InputLabel id="demo-multiple-employees-label">
              Employees
            </InputLabel>
            <Select
              labelId="demo-multiple-employees-label"
              label="Employees"
              id="multiple-employees"
              multiple
              sx={{ width: "100%" }}
              value={employees}
              {...register("employees")}
              onChange={handleEmployeesChange}
              renderValue={(selected) => (
                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                  {selected.map((value) => (
                    <Chip
                      key={value}
                      label={
                        params.employees.find((x) => x.id === value)?.name ??
                        value
                      }
                    />
                  ))}
                </Box>
              )}
              MenuProps={MenuProps}
            >
              {params.employees?.map((item) => (
                <MenuItem
                  key={item.id}
                  value={item.id}
                  style={getStyles(item.id, leaveTypes ?? [], theme)}
                >
                  {item.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Grid>
        <Grid size={{ xs: 12, sm: 12, md: 2 }}>
          <Button
            sx={{ height: "100%" }}
            type="submit"
            variant="contained"
            fullWidth
          >
            Search
          </Button>
        </Grid>
      </Grid>
    </form>
  );
};

export interface SearchLeaveRequestModel {
  dateFrom: DateTime | undefined;
  dateTo: DateTime | undefined;
  employees: string[];
  leaveTypes: string[];
}
