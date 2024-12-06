import * as React from "react";
import Box from "@mui/material/Box";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/DeleteOutlined";
import SaveIcon from "@mui/icons-material/Save";
import CancelIcon from "@mui/icons-material/Close";
import {
  GridRowsProp,
  GridRowModesModel,
  GridRowModes,
  DataGrid,
  GridColDef,
  GridActionsCellItem,
  GridEventListener,
  GridRowId,
  GridRowModel,
  GridRowEditStopReasons,
} from "@mui/x-data-grid";
import { useNotifications } from "@toolpad/core/useNotifications";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { DurationFormatter } from "../utils/DurationFormatter";
import { Duration } from "luxon";

declare module "@mui/x-data-grid" {
  interface ToolbarPropsOverrides {
    setRows: (newRows: (oldRows: GridRowsProp) => GridRowsProp) => void;
    setRowModesModel: (
      newModel: (oldModel: GridRowModesModel) => GridRowModesModel,
    ) => void;
  }
}

export function ManageLimitsTable(props: {
  employees: EmployeeDto[];
  leaveTypes: LeaveTypeDto[];
  limits: LeaveLimitDto[];
  limitOnChange: (user: LeaveLimitCell) => Promise<void>;
}) {
  const notifications = useNotifications();

  const missingEmployees = props.limits.filter(x => !props.employees.find(e => e.id === x.assignedToUserId)).map(x => ({
    id: x.assignedToUserId,
    name: x.assignedToUserId
  }))
  const employees = [
    {
      id: null,
      name: "<All>",
    },
    ...props.employees.map((x) => ({
      id: x.id,
      name: x.lastName ? `${x.lastName} ${x.firstName}` : x.name ?? x.id,
    })).concat(missingEmployees)
    .sort((l, r) => l.name?.localeCompare(r.name) ?? 0),
  ];
  const getName = (id: string): string | undefined => employees.find(x => x.id === id)?.name;

  const rowsTransformed: LeaveLimitCell[] = props.limits.map(x => ({
    ...x,
    limit: x.limit ? DurationFormatter.days(x.limit, x.workingHours) : null,
    overdueLimit: x.overdueLimit ? DurationFormatter.days(x.overdueLimit, x.workingHours) : null,
    workingHours: x.workingHours ? Duration.fromISO(x.workingHours).as("hours") : null,
    validSince: x.validSince ? new Date(Date.parse(x.validSince)) : null,
    validUntil: x.validUntil ? new Date(Date.parse(x.validUntil)) : null
  }))
  .sort((l, r) => getName(l.assignedToUserId)?.localeCompare(getName(r.assignedToUserId) ?? "") ?? 0);
  const [rows, setRows] = React.useState<GridRowsProp>(rowsTransformed);
  const [rowModesModel, setRowModesModel] = React.useState<GridRowModesModel>(
    {},
  );

  const handleRowEditStop: GridEventListener<"rowEditStop"> = (
    params,
    event,
  ) => {
    if (params.reason === GridRowEditStopReasons.rowFocusOut) {
      event.defaultMuiPrevented = true;
    }
  };

  const handleEditClick = (id: GridRowId) => () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.Edit } });
  };

  const handleSaveClick = (id: GridRowId) => async () => {
    setRowModesModel({ ...rowModesModel, [id]: { mode: GridRowModes.View } });
  };

  const handleDeleteClick = (id: GridRowId) => async () => {
    const deleteItem = rows.find((row) => (row.id === id))
    setRows(rows.filter((row) => (row.id !== id)));
    if(deleteItem) {
      const row = deleteItem as LeaveLimitCell;
      row.state = "Inactive";
      await props.limitOnChange(row);
    }
    else {
      notifications.show("Something went wrong when deleting row.", {
        severity: "error",
      });
    }
  };

  const handleCancelClick = (id: GridRowId) => () => {
    setRowModesModel({
      ...rowModesModel,
      [id]: { mode: GridRowModes.View, ignoreModifications: true },
    });
  };

  const processRowUpdate = async (updatedRow: GridRowModel) => {
    setRows(rows.map((row) => (row.id === updatedRow.id ? updatedRow : row)));
    await props.limitOnChange(updatedRow as LeaveLimitCell);
    return updatedRow;
  };

  const handleRowModesModelChange = (newRowModesModel: GridRowModesModel) => {
    setRowModesModel(newRowModesModel);
  };

  const processRowUpdateError = (e: unknown) => {
    console.warn("processRowUpdateError", e);
    notifications.show("Something went wrong when updating row.", {
      severity: "error",
    });
  };

  const columns: GridColDef[] = [
    {
      field: "assignedToUserId",
      headerName: "Employee",
      editable: true,
      minWidth: 220,
      type: "singleSelect",
      valueOptions: employees.map((x) => ({
        value: x.id,
        label: x.name,
      })),
    },
    {
      field: "leaveTypeId",
      headerName: "Leave type",
      editable: true,
      minWidth: 200,
      type: "singleSelect",
      valueOptions: props.leaveTypes.map((x) => ({
        value: x.id,
        label: x.name,
      })),
    },
    {
      field: "limit",
      headerName: "Limit (d)",
      editable: true,
      type: "number",
    },
    {
      field: "overdueLimit",
      headerName: "Overdue limit (d)",
      editable: true,
      type: "number",
    },
    {
      field: "workingHours",
      headerName: "Working hours (h)",
      editable: true,
      type: "number",
    },
    {
      field: "validSince",
      headerName: "Valid since",
      editable: true,
      type: "date",
    },
    {
      field: "validUntil",
      headerName: "Valid until",
      editable: true,
      type: "date",
    },
    {
      field: "description",
      headerName: "Description",
      editable: true,
      type: "string",
    },
    {
      field: "actions",
      type: "actions",
      headerName: "Actions",
      width: 100,
      cellClassName: "actions",
      getActions: ({ id }) => {
        const isInEditMode = rowModesModel[id]?.mode === GridRowModes.Edit;

        if (isInEditMode) {
          return [
            <GridActionsCellItem
              icon={<SaveIcon />}
              label="Save"
              sx={{
                color: "primary.main",
              }}
              onClick={handleSaveClick(id)}
            />,
            <GridActionsCellItem
              icon={<CancelIcon />}
              label="Cancel"
              className="textPrimary"
              onClick={handleCancelClick(id)}
              color="inherit"
            />,
          ];
        }

        return [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Edit"
            className="textPrimary"
            onClick={handleEditClick(id)}
            color="inherit"
          />,
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Delete"
            onClick={handleDeleteClick(id)}
            color="inherit"
          />,
        ];
      },
    },
  ];

  return (
    <Box
      sx={{
        height: 500,
        width: "100%",
        "& .actions": {
          color: "text.secondary",
        },
        "& .textPrimary": {
          color: "text.primary",
        },
      }}
    >
      <DataGrid
        rows={rows}
        columns={columns}
        editMode="row"
        rowModesModel={rowModesModel}
        onRowModesModelChange={handleRowModesModelChange}
        onRowEditStop={handleRowEditStop}
        processRowUpdate={processRowUpdate}
        onProcessRowUpdateError={processRowUpdateError}
        slotProps={{
          toolbar: { setRows, setRowModesModel },
        }}
      />
    </Box>
  );
}

export interface LeaveLimitCell {
  id: string;
  limit: number | null;
  overdueLimit: number | null;
  workingHours: number | null;
  leaveTypeId: string;
  validSince: Date | null;
  validUntil: Date | null;
  assignedToUserId: string;
  description: string | null;
  state: "Active" | "Inactive";
}