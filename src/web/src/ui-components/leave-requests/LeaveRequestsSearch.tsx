import Grid from "@mui/material/Grid";
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
import { leaveRequestsStatuses } from "../utils/Status";
import { Trans, useTranslation } from "react-i18next";
import IconButton from "@mui/material/IconButton";
import FilterListIcon from "@mui/icons-material/FilterList";
import Collapse from "@mui/material/Collapse";
import Tooltip from "@mui/material/Tooltip";

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
  }>
) => {
  const theme = useTheme();
  const now = DateTime.now().startOf("day");
  const { t } = useTranslation();
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
    now.minus({ days: 14 })
  );
  const [dateTo, setDateTo] = useState<DateTime | null>(now.plus({ days: 14 }));
  const [leaveTypes, setLeaveTypes] = useState<string[] | undefined>([]);
  const [employees, setEmployees] = useState<string[] | undefined>([]);
  const [statuses, setStatuses] = useState<string[] | undefined>([]);
  const [showAdvancedFilters, setShowAdvancedFilters] =
    useState<boolean>(false);
  const dateIsValid = (value: DateTime | null | undefined): boolean => {
    return !!value && value.isValid;
  };
  const validateDate = (value: DateTime | undefined) => {
    if (!dateIsValid(value)) return t("This is required");
  };
  const handleLeaveTypesChange = (
    event: SelectChangeEvent<typeof leaveTypes>
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
    event: SelectChangeEvent<typeof employees>
  ) => {
    const {
      target: { value },
    } = event;
    // On autofill we get a stringified value.
    const values = typeof value === "string" ? value.split(",") : value;
    setEmployees(values);
    setValue("employees", values ?? []);
  };
  const handleStatusesChange = (event: SelectChangeEvent<typeof employees>) => {
    const {
      target: { value },
    } = event;
    // On autofill we get a stringified value.
    const values = typeof value === "string" ? value.split(",") : value;
    setStatuses(values);
    setValue("statuses", values ?? []);
  };
  function getStyles(
    name: string,
    personName: readonly string[],
    theme: Theme
  ) {
    return {
      fontWeight: personName.includes(name)
        ? theme.typography.fontWeightMedium
        : theme.typography.fontWeightRegular,
    };
  }
  const onSearch = async (
    value: SearchLeaveRequestModel,
    event?: React.BaseSyntheticEvent
  ) => {
    if (!isValid) {
      return;
    }
    value.employees =
      typeof value.employees === "string"
        ? (value.employees as string).split(",")
        : value.employees;
    value.leaveTypes =
      typeof value.leaveTypes === "string"
        ? (value.leaveTypes as string).split(",")
        : value.leaveTypes;
    value.statuses =
      typeof value.statuses === "string"
        ? (value.statuses as string).split(",")
        : value.statuses;
    await params.onSubmit(value, event);
  };
  return (
    <form onSubmit={handleSubmit(onSearch)}>
      <Grid container spacing={1}>
        <Grid size={{ xs: 12, sm: 3, md: 2 }}>
          <Controller
            control={control}
            name="dateFrom"
            rules={{
              required: t("This is required"),
              validate: { required: validateDate },
            }}
            render={({ field }) => {
              return (
                <DatePicker
                  label={t("Date from *")}
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
              required: t("This is required"),
              validate: { required: validateDate },
            }}
            render={({ field }) => {
              return (
                <DatePicker
                  label={t("Date to *")}
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
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Tooltip
            title={
              showAdvancedFilters
                ? t("Hide advanced filters")
                : t("Show advanced filters")
            }
          >
            <IconButton
              onClick={() => setShowAdvancedFilters(!showAdvancedFilters)}
              color={showAdvancedFilters ? "primary" : "default"}
              sx={{
                border: "1px solid",
                borderColor: "divider",
                borderRadius: 1,
              }}
            >
              <FilterListIcon />
            </IconButton>
          </Tooltip>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Collapse in={showAdvancedFilters}>
            <Grid container spacing={1} sx={{ mt: 0.5 }}>
              <Grid size={{ xs: 12, sm: 6, md: 2 }}>
                <FormControl fullWidth>
                  <InputLabel id="multiple-leave-types-label">
                    <Trans>Leave types</Trans>
                  </InputLabel>
                  <Select
                    labelId="multiple-leave-types-label"
                    label={t("Leave types")}
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
                              params.leaveTypes?.find((x) => x.id === value)
                                ?.name ?? value
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
              <Grid size={{ xs: 12, sm: 5, md: 2 }}>
                <FormControl fullWidth>
                  <InputLabel id="multiple-employees-label">
                    <Trans>Employees</Trans>
                  </InputLabel>
                  <Select
                    labelId="multiple-employees-label"
                    label={t("Employees")}
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
                              params.employees.find((x) => x.id === value)
                                ?.name ?? value
                            }
                          />
                        ))}
                      </Box>
                    )}
                    MenuProps={MenuProps}
                  >
                    {params.employees?.map((item) => (
                      <MenuItem key={item.id} value={item.id}>
                        {item.name}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12, sm: 5, md: 2 }}>
                <FormControl fullWidth>
                  <InputLabel id="multiple-statuses-label">
                    <Trans>Statuses</Trans>
                  </InputLabel>
                  <Select
                    labelId="multiple-statuses-label"
                    label={t("Statuses")}
                    id="multiple-statuses"
                    multiple
                    sx={{ width: "100%" }}
                    value={statuses}
                    {...register("statuses")}
                    onChange={handleStatusesChange}
                    renderValue={(selected) => (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {selected.map((value) => (
                          <Chip
                            key={value}
                            label={
                              leaveRequestsStatuses.find((s) => s === value) ??
                              value
                            }
                          />
                        ))}
                      </Box>
                    )}
                    MenuProps={MenuProps}
                  >
                    {leaveRequestsStatuses.map((item) => (
                      <MenuItem key={item} value={item}>
                        {item}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Collapse>
        </Grid>
        <Grid size={{ xs: 12, sm: 2, md: 2 }}>
          <Button
            sx={{ height: "100%" }}
            type="submit"
            variant="contained"
            fullWidth
          >
            <Trans>Search</Trans>
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
  statuses: string[];
}
